using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using OpenAI;
using System.ComponentModel;

namespace GitHubTriageMcpServer
{
    [McpServerToolType]
    public class TriageLlmTool
    {
        string SummaryIssuePrompt = @"
        You are an Open Source triage assistant who is responsible for summary an issue.

        Response Format

        Always structure your response in this format:

        ## Issue Summary
        [Brief summary of the issue]

        ## Comments Summary
        [Brief summary of the issue comments]
        ";

        // Thanks to Matthew Leibowitz so much for taking the time to craft such an incredible prompt! 
        string AddLabelsPrompt = @"
        You are an Open Source triage assistant who is responsible for adding labels to issues as they are created.
        You are accurate and only pick the best match. 
        You are to NEVER make up labels.
        If an issue already has labels, you can just ignore them as they either may be wrong or the user may just want you to validate that the labels are the best ones.

        1. Fetching the Issue

        Only look at the specific issue mentioned.
        Once you have the issue details, print a short summary of it before continuing so that the user can see that it is the correct issue.
        If the issue is unclear or lacks sufficient information, note this in your response.

        2. Fetching the Labels

        Always use the GitHub repository labels.
        Validate each label with the issue contents. Some label categories may have more than one match.
        Labels are ""grouped"" using prefixes separated with a hyphen/minus (-) or with a slash (/).
        Once you have fetched the labels, let the user know how many were fetched.
        If no labels match the issue, report this clearly.

        3. Selecting a Label

        There are many labels and many may apply. Make sure to pick the ones that match the best and discard the others.
        If there are any labels that look like they may be a match, but you decided not to use it, make sure to let the user know why.
        When multiple labels apply, prioritize as follows:
        Primary issue type/category
        Component/area affected
        Priority/severity (if apparent from description)
        For ambiguous issues, select the most specific label that applies rather than a general one.

        4. Response Format

        Always structure your response in this format:

        ## Issue Summary
        [Brief summary of the issue]

        ## Labels Found
        [Number of labels found in the repository]

        ## Selected Labels
        - [Label 1]: [Brief justification]
        - [Label 2]: [Brief justification]

        ## Considered But Rejected
        - [Label]: [Reason for rejection]

        ## Additional Notes
        [Any other observations or suggestions]";

        /// <summary>
        /// Retrieves the count of open issues in a specified repository.
        /// </summary>
        /// <param name="owner">The owner of the repository.</param>
        /// <param name="repo">The name of the repository.</param>
        /// <returns>
        /// A string representing the number of open issues in the repository.
        /// </returns>
        [McpServerTool("triage_get_issues_count")]
        [Description("Fetches the count of open issues in the specified GitHub repository.")]
        public async Task<string> GetIssuesCount(string owner, string repo)
        {
            try
            {
                GitHubService gitHubService = new GitHubService();
                var issues = await gitHubService.GetIssuesAsync(owner, repo);

                return issues.Count.ToString();
            }
            catch (Exception ex)
            {
                return $"An error occurred while fetching issues Count: {ex.Message}";
            }
        }

        /// <summary>
        /// Retrieves a list of open issues in the specified repository and formats them into a string table.
        /// </summary>
        /// <param name="owner">The owner of the GitHub repository.</param>
        /// <param name="repo">The name of the GitHub repository.</param>
        /// <returns>
        /// A string-formatted table containing the issue number, title, and state for each open issue. 
        /// If no issues are found, returns a message indicating no issues.
        /// </returns>
        [McpServerTool("triage_get_issues")]
        [Description("Fetches and formats a list of open issues in the specified GitHub repository.")]
        public async Task<string> GetIssues(string owner, string repo)
        {
            try
            {
                GitHubService gitHubService = new GitHubService();
                var issues = await gitHubService.GetIssuesAsync(owner, repo);

                if (issues == null || issues.Count == 0)
                {
                    return "No issues found.";
                }

                // Format the result into a string table
                var table = new System.Text.StringBuilder();
                table.AppendLine("Issue List");
                table.AppendLine("| Number | Title                  | State     |");
                table.AppendLine("|--------|------------------------|-----------|");

                foreach (var issue in issues)
                {
                    table.AppendLine($"| {issue.Number,-6} | {issue.Title,-22} | {issue.State,-9} |");
                }

                return table.ToString();
            }
            catch (Exception ex)
            {
                return $"An error occurred while fetching issues: {ex.Message}";
            }
        }

        /// <summary>
        /// Retrieves the count of labels in the specified GitHub repository.
        /// </summary>
        /// <param name="thisServer">The MCP server instance handling the request.</param>
        /// <param name="owner">The owner of the GitHub repository.</param>
        /// <param name="repo">The name of the GitHub repository.</param>
        /// <returns>
        /// A string representing the total number of labels in the repository.
        /// </returns>
        [McpServerTool("triage_get_labels_count")]
        [Description("Fetches the total number of labels in the specified GitHub repository.")]
        public async Task<string> GetLabelsCount(string owner, string repo)
        {
            try
            {
                GitHubService gitHubService = new GitHubService();
                var labels = await gitHubService.GetLabelsAsync(owner, repo);

                return labels.Count.ToString();
            }
            catch (Exception ex)
            {
                return $"An error occurred while fetching labels Count: {ex.Message}";
            }
        }

        /// <summary>
        /// Retrieves all labels from the specified GitHub repository and formats them into a string table.
        /// </summary>
        /// <param name="owner">The owner of the GitHub repository.</param>
        /// <param name="repo">The name of the GitHub repository.</param>
        /// <returns>
        /// A string-formatted table containing the label name, description, and color for each label.
        /// </returns>
        [McpServerTool("triage_get_labels")]
        [Description("Fetches and formats a list of labels in the specified GitHub repository.")]
        public async Task<string> GetLabels(string owner, string repo)
        {
            try
            {
                GitHubService gitHubService = new GitHubService();
                var labels = await gitHubService.GetLabelsAsync(owner, repo);

                if (labels == null || labels.Count == 0)
                {
                    return "No labels found.";
                }

                // Format the labels into a string table
                var table = new System.Text.StringBuilder();
                table.AppendLine("Label List");
                table.AppendLine("| Name               | Description          | Color     |");
                table.AppendLine("|--------------------|----------------------|-----------|");

                foreach (var label in labels)
                {
                    table.AppendLine($"| {label.Name,-18} | {label.Description,-20} | {label.Color,-9} |");
                }

                return table.ToString();
            }
            catch (Exception ex)
            {
                return $"An error occurred while fetching labels: {ex.Message}";
            }
        }

        /// <summary>
        /// Retrieves a summary of a specific GitHub issue, including its title, state, author, labels, and comments.
        /// </summary>
        /// <param name="thisServer">The MCP server instance handling the request.</param>
        /// <param name="owner">The owner of the GitHub repository.</param>
        /// <param name="repo">The name of the GitHub repository.</param>
        /// <param name="issueNumber">The number of the issue to retrieve the summary for.</param>
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests and interrupt the operation if needed.
        /// </param>
        /// <returns>
        /// A string containing the summary of the issue, including its metadata, labels, and comments.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the repository owner, name, or issue number is invalid.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when an error occurs while retrieving issue details from the GitHub API.
        /// </exception>
        [McpServerTool("triage_summary_issue")]
        [Description("Fetches a summary of a GitHub issue, including metadata, labels, and comments (LLM).")]
        public async Task<string> SummaryIssueAsync(IMcpServer thisServer, string owner, string repo, int issueNumber, CancellationToken cancellationToken)
        {
            try
            {
                GitHubService gitHubService = new GitHubService();

                string labels = await gitHubService.GetLabelsAsStringAsync(owner, repo);
                string issueDetails = await gitHubService.GetIssueDetailsAsync(owner, repo, issueNumber);

                IChatClient chatClient = new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
                    .AsChatClient("gpt-4o")
                    .AsBuilder()
                    .UseFunctionInvocation()
                    .Build();

                ChatMessage[] messages =
                [
                    new(ChatRole.System, SummaryIssuePrompt),
                    new ChatMessage(ChatRole.User,
                        new List<AIContent>
                        {
                            new TextContent("Summary the issue"),
                            new TextContent($"This is the list of labels available in the repository: {labels}"),
                            new TextContent(issueDetails),
                        })
                ];

                ChatOptions options = new()
                {
                    MaxOutputTokens = 4096,
                    Temperature = 0.7f,
                };

                var response = await chatClient.GetResponseAsync(messages, options, cancellationToken);

                if (response is not null)
                {
                    return response.Text;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return $"An error occurred while creating an issue summary: {ex.Message}";
            }
        }

        /// <summary>
        /// Applies specified labels to a GitHub issue by utilizing the issue details and specific instructions provided.
        /// </summary>
        /// <param name="thisServer">The MCP server instance handling the operation.</param>
        /// <param name="owner">The owner of the GitHub repository.</param>
        /// <param name="repo">The name of the GitHub repository.</param>
        /// <param name="issueNumber">The unique number identifying the issue within the repository.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests and interrupt the operation if needed.</param>
        /// <returns>
        /// A string indicating the outcome of the operation, such as success or failure, added labels, etc.
        /// </returns>
        [McpServerTool("triage_add_labels_issue")]
        [Description("Applies specified labels to a GitHub issue, using the issue details and some specific instructions (LLM).")]
        public async Task<string> AddLabelsToIssueAsync(IMcpServer thisServer, string owner, string repo, int issueNumber, CancellationToken cancellationToken)
        {
            try
            {
                GitHubService gitHubService = new GitHubService();

                string labels = await gitHubService.GetLabelsAsStringAsync(owner, repo);
                string issueDetails = await gitHubService.GetIssueDetailsAsync(owner, repo, issueNumber);

                IChatClient chatClient = new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
                    .AsChatClient("gpt-4o")
                    .AsBuilder()
                    .UseFunctionInvocation()
                    .Build();

                ChatMessage[] messages =
                [
                    new(ChatRole.System, AddLabelsPrompt),
                    new ChatMessage(ChatRole.User,
                        new List<AIContent>
                        {
                            new TextContent("Analyze the issue details"),
                            new TextContent($"This is the list of labels available in the repository: {labels}"),
                            new TextContent(issueDetails),
                        })
                ];

                ChatOptions options = new()
                {
                    MaxOutputTokens = 4096,
                    Temperature = 0.7f,
                };
               
                var response = await chatClient.GetResponseAsync(messages, options, cancellationToken);

                if (response is not null)
                {
                    return response.Text;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return $"An error occurred while adding labels to issues: {ex.Message}";
            }
        }
    }
}