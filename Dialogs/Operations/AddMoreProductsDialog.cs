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
    public class AddMoreProductsDialog : CancelAndHelpDialog
    {
        public AddMoreProductsDialog() : base(nameof(AddMoreProductsDialog))
        {

            var waterfallSteps = new WaterfallStep[]
            {
                ProductIDStepAsync,
                ProductNameStepAsync,
                ProductPriceStepAsync,
                ProductImageURLStepAsync,
                ProductCategoryStepAsync,
                MoreProductsStepAsync,
                SummaryStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ProductIDStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Please give a product ID.")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ProductNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["ProductID"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Please give a name of the product.")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ProductPriceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["ProductName"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions
            {
                Prompt = MessageFactory.Text($"What price would you like to keep for {(string)stepContext.Values["ProductName"]}?")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ProductImageURLStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["ProductPrice"] = (int)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Please give a image URL of the product.")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ProductCategoryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["ProductImageURL"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("What category your product belongs to?"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "Television", "Laptop", "Air Conditioner", "Monitor", "Speaker", "Earphones" }),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> MoreProductsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["ProductCategory"] = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Would you like to Add more products?")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var productDetails = (ProductDetails)stepContext.Options;

            Product product = new Product()
            {
                ID = (string)stepContext.Values["ProductID"],
                Name = (string)stepContext.Values["ProductName"],
                Price = (int)stepContext.Values["ProductPrice"],
                ImageURL = (string)stepContext.Values["ProductImageURL"],
                Category = (string)stepContext.Values["ProductCategory"]
            };

            productDetails.ProductList.Add(product);

            if ((bool)stepContext.Result)
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, productDetails, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Ok."));
                return await stepContext.EndDialogAsync(productDetails, cancellationToken);
            }
        }
    }
}
