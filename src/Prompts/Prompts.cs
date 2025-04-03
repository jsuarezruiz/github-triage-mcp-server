using ModelContextProtocol.Server;
using System.ComponentModel;

namespace GitHubTriageMcpServer
{
    [McpServerPromptType]
    public class Prompts
    {
        /// <summary>
        /// Generates a prompt to add labels to a specific GitHub issue within a given repository.
        /// This method helps identify and organize issues by associating them with relevant labels.
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
        /// <returns>
        /// A string containing a structured prompt that facilitates adding labels to the specified issue.
        /// This prompt ensures that issues are appropriately categorized and prioritized.
        /// </returns>
        [McpServerPrompt]
        [Description("Generates a prompt for associating labels with GitHub issues within a repository.")]
        public string AddLabels(string owner, string repo, int issueNumber)
        {
            // Thanks to Matthew Leibowitz so much for taking the time to craft such an incredible prompt! 
            return $@"
            I want to add labels to issues as they are created.
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
        }
    }
}