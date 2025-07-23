<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# SharingMezzi.Web - Premium Frontend Project

This is a premium ASP.NET Core Razor Pages frontend application for a vehicle sharing system (SharingMezzi). The project follows modern web development practices with a focus on user experience and responsive design.

## Project Structure

- **Models/**: Data models and DTOs for API communication
- **Services/**: Service layer for API integration and business logic
- **Pages/**: Razor Pages for the frontend
- **wwwroot/**: Static files (CSS, JS, images)

## Key Features

1. **Authentication & Authorization**: JWT-based authentication with role-based access
2. **Vehicle Management**: Browse, filter, and unlock vehicles
3. **Parking Management**: View parking locations and availability
4. **User Management**: Admin panel for user administration
5. **Billing System**: Recharge wallet and view transaction history
6. **Dashboard**: KPI cards and activity overview
7. **Responsive Design**: Mobile-first approach with Bootstrap 5

## Design Guidelines

- Use **Bootstrap 5** for responsive design
- Follow **premium SaaS dashboard** aesthetics
- Implement **Inter font** for typography
- Use **Font Awesome** icons consistently
- Apply **smooth transitions** and **hover effects**
- Maintain **consistent color scheme** with CSS variables

## Code Standards

- Follow **C# naming conventions**
- Use **async/await** for all API calls
- Implement **proper error handling**
- Add **logging** for debugging
- Use **dependency injection** for services
- Follow **SOLID principles**

## API Integration

The frontend integrates with a REST API backend for:
- Authentication (`/api/auth/`)
- Vehicle management (`/api/mezzi/`)
- Parking management (`/api/parcheggi/`)
- User management (`/api/utenti/`)
- Billing (`/api/ricariche/`)

## UI Components

- **Sidebar navigation** with collapsible states
- **Dashboard cards** with KPI metrics
- **Data tables** with filtering and sorting
- **Modals** for forms and details
- **Alert notifications** for user feedback
- **Loading states** for async operations

## JavaScript Architecture

- **Modular approach** with ES6 classes
- **Event-driven** interaction handling
- **API service** abstraction
- **State management** for UI components
- **Error handling** with user-friendly messages

## Future Enhancements

- OpenStreetMap integration for vehicle/parking locations
- Real-time notifications with SignalR
- PWA capabilities for mobile experience
- Dark mode theme toggle
- Advanced analytics and reporting
