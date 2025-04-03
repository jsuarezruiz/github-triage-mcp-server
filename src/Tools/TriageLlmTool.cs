using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
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

        /// <summary>
        /// Retrieves the count of open issues in a specified repository.
        /// </summary>
        /// <param name="owner">The owner of the repository.</param>
        /// <param name="repo">The name of the repository.</param>
        /// <returns>
        /// A string representing the number of open issues in the repository.
        /// </returns>
        [McpServerTool(Name = "triage_get_issues_count")]
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
        [McpServerTool(Name = "triage_get_issues")]
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
        [McpServerTool(Name = "triage_get_labels_count")]
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
        [McpServerTool(Name = "triage_get_labels")]
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
        /// Adds labels to a specified GitHub issue in a given repository.
        /// This method interacts with a GitHub service to modify the labels associated with the issue.
        /// </summary>
        /// <param name="owner">
        /// The username or organization name of the repository owner.
        /// Example: "octocat" for GitHub's sample repositories.
        /// </param>
        /// <param name="repo">
        /// The name of the GitHub repository.
        /// Example: "Hello-World" for a sample repository.
        /// </param>
        /// <param name="issueNumber">
        /// The number identifying the issue within the repository.
        /// This is a unique number assigned to each issue.
        /// Example: For issue #42, pass 42 as the parameter.
        /// </param>
        /// <param name="labels">
        /// An array of strings representing the labels to add to the issue.
        /// Labels categorize or add context to the issue, such as "bug", "enhancement", etc.
        /// Example: new string[] { "bug", "help wanted" }.
        /// </param>
        /// <returns>
        /// A formatted string message indicating the success or failure of adding labels to the specified issue.
        /// If successful, the message will list the labels added. If an error occurs, the message will include details about the error.
        /// </returns>
        [McpServerTool(Name = "triage_add_labels_issue")]
        [Description("Applies specified labels to a GitHub issue, using the issue details and some specific instructions.")]
        public async Task<string> AddLabelsToIssue(string owner, string repo, int issueNumber, string[] labels)
        {
            try
            {
                GitHubService gitHubService = new GitHubService();
                await gitHubService.AddLabelsToIssueAsync(owner, repo, issueNumber, labels);

                // Format the success result
                var formattedLabels = string.Join(", ", labels);
                return $"Successfully added the following labels to issue #{issueNumber} in repository '{owner}/{repo}': {formattedLabels}.";
            }
            catch(Exception ex)
            {
                return $"An error occurred while adding labels to issue #{issueNumber} in repository '{owner}/{repo}': {ex.Message}";
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
        [McpServerTool(Name = "triage_summary_issue")]
        [Description("Fetches a summary of a GitHub issue, including metadata, labels, and comments (LLM).")]
        public async Task<string> SummaryIssueAsync(IMcpServer thisServer, string owner, string repo, int issueNumber, CancellationToken cancellationToken)
        {
            try
            {
                GitHubService gitHubService = new GitHubService();

                string labels = await gitHubService.GetLabelsAsStringAsync(owner, repo);
                string issueDetails = await gitHubService.GetIssueDetailsAsync(owner, repo, issueNumber);

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

                var response = await thisServer.AsSamplingChatClient().GetResponseAsync(messages, options, cancellationToken);

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
    }
}