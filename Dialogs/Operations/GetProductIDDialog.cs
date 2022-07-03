using EcommerceAdminBot.Models;
using EcommerceAdminBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceAdminBot.Dialogs.Operations
{
    public class GetProductIDDialog : CancelAndHelpDialog
    {
        CosmosDBClient _cosmosDBClient;
        public GetProductIDDialog(CosmosDBClient cosmosDBClient) : base(nameof(GetProductIDDialog))
        {
            _cosmosDBClient = cosmosDBClient;

            var waterfallSteps = new WaterfallStep[]
            {
                CategoryStepAsync,
                ChangeCategoryStepAsync,
                SummaryStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> CategoryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Select the category to get the latest product ID."),
                Choices = ChoiceFactory.ToChoices(new List<string> { "Television", "Laptop", "Air Conditioner", "Monitor", "Speaker", "Earphones" }),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ChangeCategoryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string productCategory = ((FoundChoice)stepContext.Result).Value;
            List<ProductDBDetails> productDBDetails = await _cosmosDBClient.QueryLatestCategoryItemsAsync(productCategory);
            if (productDBDetails.Count > 0)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"The latest product ID is {productDBDetails[0].Id} for product name '{productDBDetails[0].ProductName}'"), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"The {productCategory} category does not have any products."), cancellationToken);
            }

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Would you like to see other category's latest product?")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Sure."));
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }
    }
}
