using EcommerceAdminBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EcommerceAdminBot.Cards;
using EcommerceAdminBot.Models;
using EcommerceAdminBot.Services;

namespace EcommerceAdminBot.Dialogs.Operations
{
    public class AddProductsDialog : CancelAndHelpDialog
    {
        protected readonly IConfiguration Configuration;
        CosmosDBClient _cosmosDBClient;
        private readonly string CheckProductDialogID = "CheckProductDlg";
        StateService _stateService;

        public AddProductsDialog(IConfiguration configuration, CosmosDBClient cosmosDBClient, StateService stateService) : base(nameof(AddProductsDialog))
        {
            Configuration = configuration;
            _cosmosDBClient = cosmosDBClient;
            _stateService = stateService;

            var waterfallSteps = new WaterfallStep[]
            {
                GetProductIDStepAsync,
                GetLastProductIDStepAsync,
                ProductIDStepAsync,
                ProductNameStepAsync,
                ProductPriceStepAsync,
                ProductImageURLStepAsync,
                ProductCategoryStepAsync,
                MoreProductsStepAsync,
                MoreProductActStepAsync,
                SummaryStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new AddMoreProductsDialog());
            AddDialog(new GetProductIDDialog(_cosmosDBClient));
            AddDialog(new TextPrompt(CheckProductDialogID, ProductExistsValidation));

            InitialDialogId = nameof(WaterfallDialog);
        }

        
        private async Task<DialogTurnResult> GetProductIDStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("How do you like to get the product ID?"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "Enter Product ID", "View Last Product ID"}),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> GetLastProductIDStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["ProductIDOperation"] = ((FoundChoice)stepContext.Result).Value;
            string getProductID = (string)stepContext.Values["ProductIDOperation"];

            if ("View Last Product ID".Equals(getProductID))
            {
                return await stepContext.BeginDialogAsync(nameof(GetProductIDDialog), null, cancellationToken);
            }
            
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> ProductIDStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            userProfile.ValueFinder = "id";
            await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);

            return await stepContext.PromptAsync(CheckProductDialogID, new PromptOptions
            {
                Prompt = MessageFactory.Text("Please give a product ID.")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ProductNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            userProfile.ValueFinder = "ProductName";
            await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);

            stepContext.Values["ProductID"] = (string)stepContext.Result;
            
            return await stepContext.PromptAsync(CheckProductDialogID, new PromptOptions
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

        private async Task<DialogTurnResult> MoreProductActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var productDetails = (ProductDetails)stepContext.Options;

            if ((bool)stepContext.Result)
            {
                return await stepContext.BeginDialogAsync(nameof(AddMoreProductsDialog), productDetails, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(productDetails, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var productDetails = (ProductDetails)stepContext.Result;
            Product product = new Product()
            {
                ID = (string)stepContext.Values["ProductID"],
                Name = (string)stepContext.Values["ProductName"],
                Price = (int)stepContext.Values["ProductPrice"],
                ImageURL = (string)stepContext.Values["ProductImageURL"],
                Category = (string)stepContext.Values["ProductCategory"]
            };

            productDetails.ProductList.Add(product);

            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            for (int i = 0; i < productDetails.ProductList.Count; i++)
            {
                bool flag = true;

                if (await _cosmosDBClient.AddItemsToContainerAsync(productDetails.ProductList[i].ID, productDetails.ProductList[i].Name, productDetails.ProductList[i].Price, productDetails.ProductList[i].ImageURL, productDetails.ProductList[i].Category) == -1)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("The Product '" + productDetails.ProductList[i].Name + "' already exists"), cancellationToken);
                    flag = false;
                }

                if (flag)
                {
                    reply.Attachments.Add(Cards.Cards.GetHeroCard(productDetails.ProductList[i].ID, productDetails.ProductList[i].Name, productDetails.ProductList[i].Price, productDetails.ProductList[i].ImageURL).ToAttachment());
                }
                
            }

            // Send the card(s) to the user as an attachment to the activity
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Add Products operation completed. Thank you."), cancellationToken);

            return await stepContext.EndDialogAsync(productDetails, cancellationToken);
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
