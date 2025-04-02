# GitHub Triage MCP

The GitHub Triage MCP assist with the management and automation of triage workflows within GitHub repositories. It enables users to streamline issue labeling.

### Features

**Issue Triage**:
* Retrieve detailed information about GitHub issues (e.g., title, state, labels, comments).
* Identify untriaged issues, such as issues without a milestone or label.
* Fetch lists of open issues and format them for quick reference or reporting.

**Label Management**:
* Fetch all labels associated with a repository as a formatted string or table.
* Programmatically create new labels.
* Add labels to issues or pull requests based on specific criteria and instructions.

### Prerequisites

To use this server, ensure the following tools are installed on your development machine:

- .NET SDK (e.g., .NET 9)

### Setup

1. Clone this repository
2. Navigate to the project directory
3. Build the project: `dotnet build`
4. Configure with VS Code or other client:

```json
"mcp-github-triage": {
    "type": "stdio",
    "command": "dotnet",
    "args": [
        "run",
        "--project",
        "/Users/jsuarezruiz/GitHub/mobile-dev-mcp-server/src/GitHubTriageMcpServer.csproj"
    ],
    "env": {
    "GITHUB_PERSONAL_ACCESS_TOKEN": "<YOUR_TOKEN>"
    }
}
```

### Tools

* `triage_get_issues_count`: Fetches the count of open issues in the specified GitHub repository..
* `triage_get_issues`: Fetches and formats a list of open issues in the specified GitHub repository.
* `triage_get_labels_count`: Fetches the total number of labels in the specified GitHub repository..
* `triage_get_labels`: Fetches and formats a list of labels in the specified GitHub repository..
* `triage_add_labels_issue`: Applies specified labels to a GitHub issue, using the issue details and some specific instructions (LLM).

### Contributing

**I gladly welcome contributions** to help improve this project! Whether you're fixing bugs, adding new features, or enhancing documentation, your support is greatly appreciated.

1. Fork the repository
2. Create your feature branch (git checkout -b feature/my-feature)
4. Make your changes
6. Commit your changes (git commit -m 'Add a new feature')
7. Push to the branch (git push origin feature/my-feature)
8. Open a Pull Request

### License

This project is available under the MIT License.