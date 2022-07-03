using EcommerceAdminBot.Models;
using EcommerceAdminBot.Services;
using EcommerceAdminBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceAdminBot.Dialogs.Operations
{
    public class IndividualProductUpdateDialog : CancelAndHelpDialog
    {
        CosmosDBClient _cosmosDBClient;
        private readonly string CheckProductDialogID = "CheckProductDlg";
        StateService _stateService;
        public IndividualProductUpdateDialog(CosmosDBClient cosmosDBClient, StateService stateService) : base(nameof(IndividualProductUpdateDialog))
        {
            _cosmosDBClient = cosmosDBClient;
            _stateService = stateService;

            var waterfallSteps = new WaterfallStep[]
            {
                UpdateOptionsStepAsync,
                UpdateProductStepAsync,
                UpdateMoreStepAsync,
                SummaryStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new TextPrompt(CheckProductDialogID, ProductExistsValidation));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> UpdateOptionsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please choose the property you want to update for the product."),
                Choices = ChoiceFactory.ToChoices(new List<string> { "Name", "Price", "Image", "Category" }),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> UpdateProductStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Property"] = ((FoundChoice)stepContext.Result).Value;
            string property = (string)stepContext.Values["Property"];

            switch (property)
            {
                case "Name":
                    UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
                    userProfile.ValueFinder = "ProductName";
                    await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);

                    return await stepContext.PromptAsync(CheckProductDialogID, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Please give a name of the product.")
                    }, cancellationToken);
                    

                case "Price":
                    return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions
                    {
                        Prompt = MessageFactory.Text($"What price would you like to keep?")
                    }, cancellationToken);

                case "Image":
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Please give a image URL of the product.")
                    }, cancellationToken);

                case "Category":
                    return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions()
                    {
                        Prompt = MessageFactory.Text("What category you want the product to get changed?"),
                        Choices = ChoiceFactory.ToChoices(new List<string> { "Television", "Laptop", "Air Conditioner", "Monitor", "Speaker", "Earphones" }),
                    }, cancellationToken);

                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("I am sorry, that was not a correct option. Please try again."), cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> UpdateMoreStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var productList = (ProductDBDetails)stepContext.Options;
            
            string property = (string)stepContext.Values["Property"];
            bool updateFlag = false;

            switch (property)
            {
                case "Name":
                    stepContext.Values["ProductName"] = (string)stepContext.Result;
                    if (!(await _cosmosDBClient.UpdateProductCatalog(productList.Id, productList.ProductName, "ProductName", (string)stepContext.Values["ProductName"])))
                    {
                        updateFlag = true;
                    }
                    break;

                case "Price":
                    stepContext.Values["ProductPrice"] = (int)stepContext.Result;
                    int price = (int)stepContext.Values["ProductPrice"];
                    if(!(await _cosmosDBClient.UpdateProductCatalog(productList.Id, productList.ProductName, "Price", price.ToString())))
                    {
                        updateFlag = true;
                    }
                    break;

                case "Image":
                    stepContext.Values["ProductImageURL"] = (string)stepContext.Result;
                    if (!(await _cosmosDBClient.UpdateProductCatalog(productList.Id, productList.ProductName, "Image", (string)stepContext.Values["ProductImageURL"])))
                    {
                        updateFlag = true;
                    }
                    break;

                case "Category":
                    stepContext.Values["ProductCategory"] = ((FoundChoice)stepContext.Result).Value;
                    if (!(await _cosmosDBClient.UpdateProductCatalog(productList.Id, productList.ProductName, "Category", (string)stepContext.Values["ProductCategory"])))
                    {
                        updateFlag = true;
                    }
                    break;

                default:
                    break;
            }

            if (updateFlag)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Product Update Failed. Please try again later."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your product is successfully updated."), cancellationToken);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Would you like to update any other property of this product?")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var productList = (ProductDBDetails)stepContext.Options;

            if ((bool)stepContext.Result)
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, productList, cancellationToken);
            }
            else
            {
                List<ProductDBDetails> updatedProduct = await _cosmosDBClient.QueryItemWithIdAsync(productList.Id);

                var attachments = new List<Attachment>();
                var reply = MessageFactory.Attachment(attachments);
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                reply.Attachments.Add(Cards.Cards.GetHeroCard(updatedProduct[0].Id, updatedProduct[0].ProductName, updatedProduct[0].Price, updatedProduct[0].Image).ToAttachment());

                // Send the card(s) to the user as an attachment to the activity
                await stepContext.Context.SendActivityAsync(reply, cancellationToken);

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private async Task<bool> ProductExistsValidation(PromptValidatorContext<string> promptcontext, CancellationToken cancellationtoken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(promptcontext.Context, () => new UserProfile());

            string product = promptcontext.Recognized.Value;

            if (await _cosmosDBClient.CheckProductAsync(product, userProfile.ValueFinder))
            {
                await promptcontext.Context.SendActivityAsync($"The {userProfile.ValueFinder} {product} already exists. Please give different {userProfile.ValueFinder}", cancellationToken: cancellationtoken);
                return false;
            }

            return true;
        }
    }
}
