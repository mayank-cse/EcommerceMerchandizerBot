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
using AdaptiveCards;
using Newtonsoft.Json.Linq;

namespace EcommerceAdminBot.Dialogs.Operations
{
    public class AddMarketReport : CancelAndHelpDialog
    {
        protected readonly IConfiguration Configuration;
        CosmosDBClient _cosmosDBClient;
        //private readonly string CheckProductDialogID = "CheckProductDlg";
        StateService _stateService;

        public AddMarketReport(IConfiguration configuration, CosmosDBClient cosmosDBClient, StateService stateService) : base(nameof(AddMarketReport))
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
                //ProductPriceStepAsync,
                ProductImageURLStepAsync,
                messageStepAsync,
                ProductCategoryStepAsync,
                MoreProductsStepAsync,
                //MoreProductActStepAsync,
                SummaryStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new AddMoreProductsDialog());
            //AddDialog(new GetProductIDDialog(_cosmosDBClient));
            //AddDialog(new TextPrompt(CheckProductDialogID, ProductExistsValidation));

            InitialDialogId = nameof(WaterfallDialog);
        }


        private async Task<DialogTurnResult> GetProductIDStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Specify the information you want to report with Distributor / Store details <Store Name/ Distributor Name>"),
                //Choices = ChoiceFactory.ToChoices(new List<string> { "Enter Product ID", "View Last Product ID" }),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> GetLastProductIDStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["StoreName"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("Share the Market Area / location? Ex. <Address - LG Shoppe Mayur Vihar-Delhi-110091>"),
                //Choices = ChoiceFactory.ToChoices(new List<string> { "Enter Product ID", "View Last Product ID" }),
            }, cancellationToken);

        }

        private async Task<DialogTurnResult> ProductIDStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            userProfile.ValueFinder = "id";
            await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);


            stepContext.Values["location"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions()
            {
                Prompt = MessageFactory.Text("What category of product your report is targetting? You can mention 'General' for all category"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "Laptop", "Monitor", "Projector", "General" }),
            }, cancellationToken);
            
        }

        private async Task<DialogTurnResult> ProductNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            userProfile.ValueFinder = "ProductName";
            await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            stepContext.Values["ProductCategory"] = ((FoundChoice)stepContext.Result).Value;

            
            
            List<string> operationList = new List<string> { "Competition Pricing", "New Product Launch", "Scheme / Festival Offer", "Any Other Information" };
            // Create card

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                // Use LINQ to turn the choices into submit actions
                Actions = operationList.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Data = choice,  // This will be a string
                }).ToList<AdaptiveAction>(),
            };
            /*//Typing... Message
            await stepContext.Context.SendActivitiesAsync(
                new Activity[] {
                new Activity { Type = ActivityTypes.Typing },
                new Activity { Type = "delay", Value= 5000 },
                MessageFactory.Text("Finished typing", "Finished typing"),
                    },
                    cancellationToken);
            // Prompt*/
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    // Convert the AdaptiveCard to a JObject
                    Content = JObject.FromObject(card),
                }),
                Choices = ChoiceFactory.ToChoices(operationList),
                // Don't render the choices outside the card
                Style = ListStyle.None,
            },
                cancellationToken);
        }


        private async Task<DialogTurnResult> ProductImageURLStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            userProfile.ValueFinder = "ProductName";
            await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);

            stepContext.Values["CompanyR"] = ((FoundChoice)stepContext.Result).Value;
            //stepContext.Values["CompanyR"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("If you have any links please share. Ex. <Https://lg.com>. You can mention 'SKIP' ")
            }, cancellationToken);
        }
        private async Task<DialogTurnResult> messageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["ProductImageURL"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Please mention competitor brand and model with RRP Eg. <Samsung has launched Diwali offer>")
            }, cancellationToken);
        }
        private async Task<DialogTurnResult> ProductCategoryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["message"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("If you have any picture do share(currently share Image URL)")
            }, cancellationToken);
        }


        private async Task<DialogTurnResult> MoreProductsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["PictureInt"] = (string)stepContext.Result;


            var marketDetails = (MarketDetails)stepContext.Options;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Add Products operation completed. Thank you."), cancellationToken);

            return await stepContext.NextAsync(marketDetails, cancellationToken);
        }



        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            stepContext.Values["MerchandiserEmail"] = userProfile.Email;
            var marketDetails = (MarketDetails)stepContext.Result;
            ProductM product = new ProductM()
            {
                Email = (string)stepContext.Values["MerchandiserEmail"],
                //ID = (string)stepContext.Values["ProductID"],
                SName = (string)stepContext.Values["StoreName"],
                location = (string)stepContext.Values["location"],
                Photo = (string)stepContext.Values["PictureInt"],
                informationCategory= (string)stepContext.Values["CompanyR"],
                //Photo = (int)stepContext.Values["ProductPrice"],
                ImageURL = (string)stepContext.Values["ProductImageURL"],
                Category = (string)stepContext.Values["ProductCategory"],
                textmessage = (string)stepContext.Values["message"]
            };

            marketDetails.ProductList.Add(product);

            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Add Products operation completed. Thank you."), cancellationToken);
            await _cosmosDBClient.CreateDBConnection(Configuration["CosmosEndPointURI"], Configuration["CosmosPrimaryKey"], Configuration["CosmosDatabaseIdMarketReport"], Configuration["CosmosContainerIdMarketReport"], Configuration["CosmosPartitionKeyMarketReport"]);

            for (int i = 0; i < marketDetails.ProductList.Count; i++)
            {
                bool flag = true;

                if (await _cosmosDBClient.AddItemsToContainerAsync(marketDetails.ProductList[i].Email, marketDetails.ProductList[i].SName, marketDetails.ProductList[i].location, marketDetails.ProductList[i].Photo, marketDetails.ProductList[i].informationCategory, marketDetails.ProductList[i].ImageURL, marketDetails.ProductList[i].Category, marketDetails.ProductList[i].textmessage) == -1)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("The Product '" + marketDetails.ProductList[i].Email + "' already exists"), cancellationToken);
                    flag = false;
                }

                if (flag)
                {
                    reply.Attachments.Add(Cards.Cards.GetHeroCard(marketDetails.ProductList[i].Email, marketDetails.ProductList[i].SName, marketDetails.ProductList[i].location, marketDetails.ProductList[i].ImageURL, marketDetails.ProductList[i].textmessage).ToAttachment());
                }

            }

            // Send the card(s) to the user as an attachment to the activity
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Add Products operation completed. Thank you."), cancellationToken);

            return await stepContext.EndDialogAsync(marketDetails, cancellationToken);
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
