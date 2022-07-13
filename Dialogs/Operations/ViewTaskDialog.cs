using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EcommerceAdminBot.Utilities;

namespace EcommerceAdminBot.Dialogs.Operations
{
    public class ViewTaskDialog : ComponentDialog
    {
        protected readonly IConfiguration Configuration;
        private readonly CosmoDBClientToDo _cosmosDBClient;
        public ViewTaskDialog(IConfiguration configuration, CosmoDBClientToDo cosmosDBClient) : base(nameof(ViewTaskDialog))
        {
            Configuration = configuration;
            _cosmosDBClient = cosmosDBClient;

            var waterfallSteps = new WaterfallStep[]
            {
                ShowTasksStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ShowTasksStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            List<ToDoTask> toDoTasks = await _cosmosDBClient.QueryItemsAsync(User.UserID, Configuration["CosmosEndPointURI"], Configuration["CosmosPrimaryKey"], Configuration["CosmosDatabaseIdToDo"], Configuration["CosmosContainerIDToDo"], Configuration["CosmosPartitionKeyToDo"]);
            if (toDoTasks.Count == 0)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("You don't have any tasks added."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please find the below tasks you provided."), cancellationToken);
            /*for (int i = 0; i < toDoTasks.Count; i++)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(toDoTasks[i].Task), cancellationToken);
            }*/
            List<string> taskList = new List<string>();
            for (int i = 0; i < toDoTasks.Count; i++)
            {
                taskList.Add(toDoTasks[i].Task);
            }

            //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please select the tasks you want to delete."), cancellationToken);

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                // Use LINQ to turn the choices into submit actions
                Actions = taskList.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Data = choice,  // This will be a string
                }).ToList<AdaptiveAction>(),
            };
            /*
            //Typing... Message
            await stepContext.Context.SendActivitiesAsync(
                new Microsoft.Bot.Schema.Activity[] {
                new Microsoft.Bot.Schema.Activity { Type = ActivityTypes.Typing },
                new Microsoft.Bot.Schema.Activity { Type = "delay", Value= 5000 },
                MessageFactory.Text("Finished typing", "Finished typing"),
                    },
                    cancellationToken);
            */
            // Prompt
            await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Microsoft.Bot.Schema.Activity)MessageFactory.Attachment(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    // Convert the AdaptiveCard to a JObject
                    Content = JObject.FromObject(card),
                }),
                Choices = ChoiceFactory.ToChoices(taskList),
                // Don't render the choices outside the card
                Style = ListStyle.None,
            },
                cancellationToken);
            /*//Typing... Message
            await stepContext.Context.SendActivitiesAsync(
                new Microsoft.Bot.Schema.Activity[] {
                new Microsoft.Bot.Schema.Activity { Type = ActivityTypes.Typing },
                new Microsoft.Bot.Schema.Activity { Type = "delay", Value= 8000 },
                MessageFactory.Text("Finished typing", "Finished typing"),
                    },
                    cancellationToken);
            */

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
