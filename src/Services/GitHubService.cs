using Octokit;
using System.Xml.Linq;

/// <summary>
/// Provides methods for interacting with the GitHub API using the Octokit library.
/// Includes functionalities to fetch issues, labels, create labels, and get comment counts.
/// </summary>
public class GitHubService
{
    // Private instance of the GitHubClient used for API interactions
    readonly GitHubClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubService"/> class.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the personal access token is null or empty.</exception>
    public GitHubService()
    {
        var personalAccessToken = Environment.GetEnvironmentVariable("GITHUB_PERSONAL_ACCESS_TOKEN");

        if (string.IsNullOrWhiteSpace(personalAccessToken))
            throw new ArgumentException("Personal access token is required.", nameof(personalAccessToken));

        _client = new GitHubClient(new ProductHeaderValue("GitHubService"))
        {
            Credentials = new Credentials(personalAccessToken)
        };
    }

    /// <summary>
    /// Retrieves all issues from the specified repository.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repo">The name of the repository.</param>
    /// <returns>A list of issues in the repository.</returns>
    /// <exception cref="ArgumentException">Thrown when owner or repo parameters are invalid.</exception>
    /// <exception cref="Exception">Thrown when the API call fails.</exception>
    public async Task<IReadOnlyList<Issue>> GetIssuesAsync(string owner, string repo)
    {
        ValidateRepositoryArguments(owner, repo);

        try
        {
            return await _client.Issue.GetAllForRepository(owner, repo);
        }
        catch (Exception ex)
        {
            LogError("Failed to fetch issues", ex);
            throw;
        }
    }

    /// <summary>
    /// Retrieves issues without a milestone from the specified repository (triage issues).
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repo">The name of the repository.</param>
    /// <returns>A list of issues that need triage.</returns>
    /// <exception cref="ArgumentException">Thrown when owner or repo parameters are invalid.</exception>
    /// <exception cref="Exception">Thrown when the API call fails.</exception>
    public async Task<IReadOnlyList<Issue>> GetIssuesToTriageAsync(string owner, string repo)
    {
        ValidateRepositoryArguments(owner, repo);

        try
        {
            // Fetch all issues and filter out those with a milestone
            var issues = await _client.Issue.GetAllForRepository(owner, repo, new RepositoryIssueRequest
            {
                State = ItemStateFilter.Open
            });

            var triageIssues = issues.Where(issue => issue.Milestone is null).ToList();

            return triageIssues;
        }
        catch (Exception ex)
        {
            LogError("Failed to fetch triage issues", ex);
            throw;
        }
    }

    /// <summary>
    /// Retrieves the details of a specific GitHub issue and formats it as a string.
    /// </summary>
    /// <param name="owner">The owner of the GitHub repository.</param>
    /// <param name="repo">The name of the GitHub repository.</param>
    /// <param name="issueNumber">The number of the issue to retrieve.</param>
    /// <returns>
    /// A string containing the issue's details, including title, state, author, and body content.
    /// </returns>
    public async Task<string> GetIssueDetailsAsync(string owner, string repo, int issueNumber)
    {
        try
        {
            // Get the issue details
            var issue = await _client.Issue.Get(owner, repo, issueNumber);

            // Format the issue details into a string
            var issueDetails = new System.Text.StringBuilder();
            issueDetails.AppendLine("Issue Details:");
            issueDetails.AppendLine($"Title: {issue.Title}");
            issueDetails.AppendLine($"State: {issue.State.StringValue}");
            issueDetails.AppendLine($"Author: {issue.User.Login}");
            issueDetails.AppendLine($"Created At: {issue.CreatedAt.ToString("f")}");
            issueDetails.AppendLine($"Labels: {string.Join(", ", issue.Labels.Select(label => label.Name))}");
            issueDetails.AppendLine("Body:");
            issueDetails.AppendLine(string.IsNullOrEmpty(issue.Body) ? "No description provided." : issue.Body);

            // Get all comments associated with the issue
            var comments = await _client.Issue.Comment.GetAllForIssue(owner, repo, issueNumber);

            // Add all comments to the formatted string
            issueDetails.AppendLine("Comments:");

            if (comments.Count == 0)
            {
                issueDetails.AppendLine("No comments available.");
            }
            else
            {
                foreach (var comment in comments)
                {
                    issueDetails.AppendLine($"- [{comment.User.Login} at {comment.CreatedAt:f}]: {comment.Body}");
                }
            }

            return issueDetails.ToString();
        }
        catch (Exception ex)
        {
            LogError("An error occurred while retrieving the issue", ex);
            throw;
        }
    }

    /// <summary>
    /// Retrieves all labels from the specified repository.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repo">The name of the repository.</param>
    /// <returns>A list of labels in the repository.</returns>
    /// <exception cref="ArgumentException">Thrown when owner or repo parameters are invalid.</exception>
    /// <exception cref="Exception">Thrown when the API call fails.</exception>
    public async Task<IReadOnlyList<Label>> GetLabelsAsync(string owner, string repo)
    {
        ValidateRepositoryArguments(owner, repo);

        try
        {
            return await _client.Issue.Labels.GetAllForRepository(owner, repo);
        }
        catch (Exception ex)
        {
            LogError("Failed to fetch labels", ex);
            throw;
        }
    }

    /// <summary>
    /// Retrieves all labels from the specified GitHub repository and returns them as a single comma-separated string.
    /// </summary>
    /// <param name="owner">The owner of the GitHub repository.</param>
    /// <param name="repo">The name of the GitHub repository.</param>
    /// <returns>
    /// A comma-separated string containing the names of all labels in the repository.
    /// If no labels are found, returns an empty string.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the repository owner or name is invalid.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown when an error occurs while fetching labels from the GitHub API.
    /// </exception>
    public async Task<string> GetLabelsAsStringAsync(string owner, string repo)
    {
        ValidateRepositoryArguments(owner, repo);

        try
        {
            var labels = await _client.Issue.Labels.GetAllForRepository(owner, repo);

            // Convert the label names to a single string separated by commas
            return string.Join(", ", labels.Select(label => label.Name));
        }
        catch (Exception ex)
        {
            LogError("Failed to fetch labels", ex);
            throw;
        }
    }

    /// <summary>
    /// Creates a new label in the specified repository.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repo">The name of the repository.</param>
    /// <param name="name">The name of the new label.</param>
    /// <param name="color">The color of the new label (HEX format).</param>
    /// <param name="description">An optional description for the label.</param>
    /// <returns>The created label.</returns>
    /// <exception cref="ArgumentException">Thrown when owner, repo, name, or color parameters are invalid.</exception>
    /// <exception cref="Exception">Thrown when the API call fails.</exception>
    public async Task<Label> CreateLabelAsync(string owner, string repo, string name, string color, string description = "")
    {
        ValidateRepositoryArguments(owner, repo);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Label name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(color))
            throw new ArgumentException("Label color is required.", nameof(color));

        try
        {
            var newLabel = new NewLabel(name, color) { Description = description };
            return await _client.Issue.Labels.Create(owner, repo, newLabel);
        }
        catch (Exception ex)
        {
            LogError($"Failed to create label '{name}'", ex);
            throw;
        }
    }

    /// <summary>
    /// Adds labels to an existing GitHub issue.
    /// </summary>
    /// <param name="owner">The repository owner's username or organization name.</param>
    /// <param name="repoName">The repository name.</param>
    /// <param name="issueNumber">The issue number to which labels will be added.</param>
    /// <param name="labels">An array of labels to add to the issue.</param>
    /// <returns>A task representing the asynchronous operation.</returns>

    public async Task AddLabelsToIssueAsync(string owner, string repoName, int issueNumber, string[] labels)
    {
        try
        {
            await _client.Issue.Labels.AddToIssue(owner, repoName, issueNumber, labels);
        }
        catch (Exception ex)
        {
            LogError($"Failed to add labels to the issue '{issueNumber}'", ex);
            throw;
        }
    }

    /// <summary>
    /// Retrieves the number of comments for a specific issue in the repository.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repo">The name of the repository.</param>
    /// <param name="issueNumber">The number of the issue.</param>
    /// <returns>The count of comments on the issue.</returns>
    /// <exception cref="ArgumentException">Thrown when owner, repo, or issueNumber parameters are invalid.</exception>
    /// <exception cref="Exception">Thrown when the API call fails.</exception>
    public async Task<int> GetCommentCountAsync(string owner, string repo, int issueNumber)
    {
        ValidateRepositoryArguments(owner, repo);

        if (issueNumber <= 0)
            throw new ArgumentException("Issue number must be greater than zero.", nameof(issueNumber));

        try
        {
            var issueComments = await _client.Issue.Comment.GetAllForIssue(owner, repo, issueNumber);
            return issueComments.Count;
        }
        catch (Exception ex)
        {
            LogError($"Failed to fetch comments for issue #{issueNumber}", ex);
            throw;
        }
    }

    /// <summary>
    /// Validates that the repository owner and name are not null or empty.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repo">The name of the repository.</param>
    /// <exception cref="ArgumentException">Thrown when owner or repo is null or empty.</exception>
    static void ValidateRepositoryArguments(string owner, string repo)
    {
        if (string.IsNullOrWhiteSpace(owner))
            throw new ArgumentException("Repository owner is required.", nameof(owner));

        if (string.IsNullOrWhiteSpace(repo))
            throw new ArgumentException("Repository name is required.", nameof(repo));
    }

    /// <summary>
    /// Logs an error message to the console.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <param name="ex">The exception that occurred.</param>
    static void LogError(string message, Exception ex)
    {
        Console.WriteLine($"{message}: {ex.Message}");
    }
}
