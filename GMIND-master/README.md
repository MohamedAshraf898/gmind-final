# Educational Game Store

A comprehensive ASP.NET MVC platform for educational games with multi-role user management, class systems, and mobile app integration.

## Features

### 🎮 Core Functionality
- **Game Store**: Browse, purchase, and download educational games
- **Multi-Role System**: Support for Students, Teachers, Parents, and Admins
- **App Verification**: Secure token-based verification system for mobile apps
- **Class Management**: Teachers can create classes, students can join using codes
- **Parent Dashboard**: Parents can monitor their children's progress and purchases

### 🔐 Authentication & Security
- ASP.NET Identity integration
- OAuth support (Google, Facebook)
- Role-based authorization
- Secure token generation for app verification
- Parent-student relationship management

### 📚 Educational Features
- **Game Categories**: Math, Science, Language, History, Geography
- **Review System**: Students can rate and review games
- **Class Integration**: Link games to educational classes
- **Progress Tracking**: Monitor student engagement and purchases

### 🎪 Events & Content Management
- **Event Registration**: Users can register and pay for educational events
- **Dynamic Content**: Admin-managed homepage sections and topics
- **Responsive Design**: Modern Bootstrap-based UI

## Technology Stack

- **Backend**: ASP.NET Core MVC (.NET 9)
- **Database**: Entity Framework Core with SQL Server
- **Authentication**: ASP.NET Identity + OAuth
- **Frontend**: Razor Views, Bootstrap 5, Bootstrap Icons
- **Styling**: Custom CSS with modern animations

## Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server (LocalDB for development)
- Visual Studio Code or Visual Studio

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Gamingv1
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database connection string** (if needed)
   Edit `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EducationalGameStore;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Create and update database**
   ```bash
   dotnet ef database update
   ```

5. **Configure OAuth (Optional)**
   Update `appsettings.json` with your OAuth credentials:
   ```json
   {
     "Authentication": {
       "Google": {
         "ClientId": "your-google-client-id",
         "ClientSecret": "your-google-client-secret"
       },
       "Facebook": {
         "AppId": "your-facebook-app-id",
         "AppSecret": "your-facebook-app-secret"
       }
     }
   }
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

7. **Access the application**
   Open your browser and navigate to `http://localhost:5000`

## Database Schema

### Core Tables
- **AspNetUsers**: User accounts with custom fields (Role, StudentCode, etc.)
- **OAuthAccounts**: OAuth provider linkages
- **Games**: Educational game catalog
- **GameCategories**: Game categorization
- **GamePurchases**: Purchase transactions
- **GameTokens**: App verification tokens

### Educational Tables
- **Classes**: Teacher-created classes
- **ClassStudents**: Student enrollments
- **ParentsStudents**: Parent-child relationships
- **Events**: Educational events
- **EventRegistrations**: Event sign-ups

### Content Management
- **DynamicSections**: Homepage content sections
- **DynamicTopics**: Content within sections

## User Roles

### 👨‍🎓 Student
- Browse and purchase games
- Join classes using codes
- Review purchased games
- Register for events

### 👨‍🏫 Teacher
- Create and manage classes
- Generate join codes
- View enrolled students
- Monitor class activity

### 👨‍👩‍👧‍👦 Parent
- Link to children using student codes
- Monitor children's purchases and progress
- View children's class enrollments
- Register for family events

### 🔧 Admin
- Manage games and categories
- Create dynamic homepage content
- View system analytics
- Manage users and system settings

## App Verification Workflow

1. **Purchase**: User buys game on website
2. **Download**: User downloads game from app store
3. **Verification**: App redirects to website verification endpoint
4. **Token Generation**: System generates secure token for valid purchases
5. **App Access**: Token unlocks game features in mobile app

## API Endpoints

### Game Verification
- `GET /Games/VerifyApp?gameId={id}` - Verify game ownership and generate token

### Authentication
- `POST /Account/Login` - User login
- `POST /Account/Register` - User registration
- `GET /Account/ExternalLogin` - OAuth login

## Development

### Adding New Features
1. Create models in `/Models`
2. Update `ApplicationDbContext`
3. Create migration: `dotnet ef migrations add <MigrationName>`
4. Update database: `dotnet ef database update`
5. Create controllers and views

### Code Style
- Follow C# naming conventions
- Use async/await for database operations
- Add XML documentation for public methods
- Implement proper error handling

## Deployment

### Production Considerations
- Update connection strings for production database
- Configure OAuth providers
- Set up HTTPS
- Configure logging
- Set up backup strategies

### Environment Variables
Set these in production:
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection`
- `Authentication__Google__ClientId`
- `Authentication__Google__ClientSecret`

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions:
- Create an issue in the repository
- Check the documentation
- Review the code comments

---

**Built with ❤️ for educational gaming**
