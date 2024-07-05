
# Gmail Traduction

Gmail Traduction is a Blazor WebApp that connects to Gmail using the Google Gmail API. It displays the first emails from a specific domain and provides detailed information about each email, including an estimated time for various services.

## Features

- Connect to Gmail using the Google Gmail API.
- Display the first emails from a specific domain.
- View detailed information about each email, including service type, deadline, total words, standard words, and estimated time.

## Prerequisites

- .NET 6.0 SDK or later
- Gmail API credentials

## Setup

### Step 1: Get Gmail API Credentials

1. Go to the [Google Cloud Console](https://console.cloud.google.com/).
2. Create a new project.
3. Enable the Gmail API for your project.
4. Create OAuth 2.0 credentials.
5. Download the `client_secret.json` file and save it in the `wwwroot` directory of your project.

### Step 2: Configure the Application

1. Open the `appsettings.json` file and ensure the following configuration:

\`\`\`json
{
  "RedirectUri": "http://localhost:5000/signin-google"
}
\`\`\`

### Step 3: Register the Redirect URI

1. Go to the [Google Cloud Console](https://console.cloud.google.com/).
2. Open the Credentials page.
3. Edit the OAuth 2.0 Client ID.
4. Add the redirect URI: `http://localhost:5000/signin-google`.

### Step 4: Update the Domain

1. Open the `Gmail.razor` file.
2. Change the domain in the `GetEmails` method to the desired domain.

### Step 5: Run the Application

1. Build and run the application:

\`\`\`bash
dotnet build
dotnet run
\`\`\`

2. Open your browser and navigate to `http://localhost:5000`.

## Usage

1. Click the "Gmail" in the menu.
2. Click the "Get Emails" button to fetch the first emails from the specified domain.
3. Click the "Read More" button on a card to view detailed information about the email.
4. Click the "Back" button to return to the main page.

## License

This project is licensed under the MIT License.

## Contact

For any questions or suggestions, please contact the project maintainer at [adesso.sevici@gmail.com].

## Screenshot
![image](https://github.com/adessolv/Gmail_Traduction/assets/148480/e7945a06-e79a-4e36-b1ae-a6f177d1e0f6)

