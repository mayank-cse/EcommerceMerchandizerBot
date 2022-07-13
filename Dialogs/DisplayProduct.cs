// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.11.1

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EcommerceAdminBot.Dialogs.Operations;
using EcommerceAdminBot.Utilities;
using EcommerceAdminBot.Models;
using EcommerceAdminBot.Services;
using ToDoBot.Dialogs.Operations;

namespace EcommerceAdminBot.Dialogs
{
    public class DisplayProduct : ComponentDialog
    {
        //private readonly ToDoLUISRecognizer _luisRecognizer;
        //protected readonly ILogger Logger;
        protected readonly IConfiguration Configuration;
        CosmosDBClient _cosmosDBClient;
        //private readonly string CheckProductDialogID = "CheckProductDlg";
        StateService _stateService;

        public DisplayProduct(IConfiguration configuration, CosmosDBClient cosmosDBClient, StateService stateService) : base(nameof(DisplayProduct))
        {
            Configuration = configuration;
            _cosmosDBClient = cosmosDBClient;
            _stateService = stateService;
        
            // _luisRecognizer = luisRecognizer;
            //Logger = logger;
            Configuration = configuration;
            _cosmosDBClient = cosmosDBClient;
            _stateService = stateService;
            var waterfallSteps = new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            //AddDialog(new TextPrompt(UserValidationDialogID, UserValidation));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new AddProductsDialog(Configuration, cosmosDBClient, _stateService));
            AddDialog(new UpdateProductDialog(_cosmosDBClient, _stateService));
            AddDialog(new RemoveProductsDialog(_cosmosDBClient, _stateService));
            AddDialog(new ViewAllProductsDialog(_cosmosDBClient, _stateService));
            //AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]

            //AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        
        

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await _cosmosDBClient.CreateDBConnection(Configuration["CosmosEndPointURI"], Configuration["CosmosPrimaryKey"], Configuration["CosmosDatabaseId"], Configuration["CosmosContainerID"], Configuration["CosmosPartitionKey"]);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("How can I help you today?"), cancellationToken);
            List<string> operationList = new List<string> { "Add Products", "Update Product", "Remove Products", "View All Products", "Exit" };
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
            /*/Typing... Message
            await stepContext.Context.SendActivitiesAsync(
                new Activity[] {
                new Activity { Type = ActivityTypes.Typing },
                new Activity { Type = "delay", Value= 5000 },
                MessageFactory.Text("Finished typing", "Finished typing"),
                    },
                    cancellationToken);
            */// Prompt
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
        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Operation"] = ((FoundChoice)stepContext.Result).Value;
            string operation = (string)stepContext.Values["Operation"];
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("You have selected - " + operation), cancellationToken);
            if ("Exit".Equals(operation))
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            if ("Add Products".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(AddProductsDialog), new ProductDetails(), cancellationToken);
            }
            else if ("Update Product".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(UpdateProductDialog), new ProductDetails(), cancellationToken);
            }
            else if ("Remove Products".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(RemoveProductsDialog), new ProductDetails(), cancellationToken);
            }
            else if("View All Products".Equals(operation))
            {
                return await stepContext.BeginDialogAsync(nameof(ViewAllProductsDialog), new ProductDetails(), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("The selected option not found."), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            // Restart the main dialog with a different message the second time around
            var promptMessage = "Thank You for Your timely updated. What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }

    }
}
