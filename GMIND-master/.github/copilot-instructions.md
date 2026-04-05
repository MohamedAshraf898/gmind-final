# Copilot Instructions for Educational Game Store

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

## Project Overview
This is an ASP.NET MVC Educational Game Store platform with Entity Framework Core and SQL Server. The application supports:

- User authentication with ASP.NET Identity and OAuth (Google, Facebook)
- Multi-role system (Admin, Teacher, Parent, Student)
- Game purchasing and mobile app verification system
- Class management for teachers and students
- Event registration and payment processing
- Dynamic homepage content management
- Parent-student relationship management

## Key Technologies
- ASP.NET MVC (.NET 6+)
- Entity Framework Core
- SQL Server
- ASP.NET Identity
- OAuth Authentication (Google, Facebook)
- Razor Views
- Bootstrap/CSS

## Architecture Patterns
- Follow MVC pattern strictly
- Use Repository pattern for data access
- Implement proper error handling and validation
- Use dependency injection for services
- Follow RESTful conventions for API endpoints

## Database Design
The project uses the following main entities:
- Users (with ASP.NET Identity)
- OAuthAccounts
- Games, GameCategories, GameImages, GameReviews
- GamePurchases, GameTokens
- Classes, ClassStudents
- ParentsStudents
- Events, EventRegistrations
- DynamicSections, DynamicTopics

## Security Considerations
- Always validate user permissions before actions
- Use proper authorization attributes
- Secure token generation for app verification
- Protect sensitive endpoints with authentication
- Validate all user inputs

## Code Style
- Use meaningful variable and method names
- Add XML documentation for public methods
- Follow C# naming conventions
- Use async/await for database operations
- Implement proper error logging
