# Parque Temático Virtual - Angular Frontend

Complete Angular 19.2.2 frontend implementation for the theme park management system.

## Features Implemented

### Core Infrastructure
- ✅ Angular 19.2.2 with standalone components
- ✅ Bootstrap 5 styling with ng-bootstrap
- ✅ JWT authentication with HTTP interceptor
- ✅ Role-based routing guards (Administrator, Operator, Visitor)
- ✅ TypeScript interfaces for all 30+ DTOs
- ✅ Reactive Forms for all user inputs
- ✅ Environment configuration (development/production)

### Authentication
- ✅ Login component with validation
- ✅ Visitor registration component
- ✅ JWT token management with localStorage
- ✅ Role-based redirection after login

### Administrator Features
- ✅ Dashboard with stats overview
- ✅ Attractions management (CRUD operations)
- ✅ Events management with attraction selection
- ✅ User management (create admin/operator accounts)
- ✅ Visits reports with ng2-charts visualization
- ✅ Strategy management and Top 10 visitors
- ✅ Filtering and search functionality

### Operator Features
- ✅ Dashboard with real-time capacity monitoring (auto-refresh every 30s)
- ✅ Entry/Exit registration for attractions
- ✅ Incident management (add/remove incidents)
- ✅ Live attraction status with capacity indicators

### Visitor Features
- ✅ Dashboard with tickets overview
- ✅ Browse attractions with search
- ✅ Browse events with search
- ✅ Ticket purchase (General and Event Special)
- ✅ My Tickets with QR code display (angularx-qrcode)
- ✅ Profile management

### Shared Components
- ✅ Responsive navbar with role-based menu items
- ✅ Loading spinner
- ✅ Unauthorized access page
- ✅ Custom pipes for enum display

## Project Structure

```
frontend/src/app/
├── core/
│   ├── guards/         # Auth and role guards
│   ├── interceptors/   # JWT interceptor
│   ├── models/         # TypeScript interfaces
│   └── services/       # All API services
├── shared/
│   ├── components/     # Reusable components
│   └── pipes/          # Custom pipes
└── features/
    ├── auth/           # Login & registration
    ├── administrator/  # Admin features
    ├── operator/       # Operator features
    └── visitor/        # Visitor features
```

## Running the Application

### Prerequisites
- Node.js 18+
- npm 10+

### Installation
```bash
cd frontend
npm install
```

### Development Server
```bash
npm start
# Navigate to http://localhost:4200
```

### Production Build
```bash
npm run build
# Output in dist/frontend
```

## API Integration

The frontend connects to the ASP.NET Core API at:
- **Development**: `http://localhost:5020/api`
- **Production**: Configure in `src/environments/environment.ts`

## Services

All backend API controllers are consumed through dedicated Angular services:

- **AuthService** - Login, register, logout, token management
- **AttractionService** - CRUD operations, entry/exit, capacity, visits reports
- **EventService** - CRUD operations for events
- **TicketService** - Purchase and view tickets
- **UserService** - User management
- **IncidentService** - Incident management
- **StrategyService** - Strategy management and top visitors

## Key Libraries

- **Angular 19.2.2** - Framework
- **Bootstrap 5** - UI styling
- **ng-bootstrap** - Bootstrap components for Angular
- **ng2-charts** - Chart.js wrapper for Angular (reports visualization)
- **angularx-qrcode** - QR code generation for tickets
- **Chart.js 4** - Charting library
- **RxJS** - Reactive programming

## Features Highlights

### Real-Time Updates
- Operator dashboard auto-refreshes capacity every 30 seconds
- Live capacity indicators with color-coded progress bars

### Advanced Filtering
- Search attractions by name/description
- Filter attractions by type
- Search events by name
- Date range filtering for reports

### QR Code Generation
- Each ticket displays a unique QR code
- QR codes can be scanned for entry verification
- Visual display with angularx-qrcode library

### Charts & Analytics
- Visitor statistics with bar charts
- Attraction visits over time
- Average stay duration metrics

### Responsive Design
- Mobile-friendly Bootstrap layout
- Adaptive navigation
- Card-based interfaces

## Build Status

✅ **Build Successful**

The application builds without errors. Minor warnings present:
- Bootstrap SCSS import deprecation (cosmetic, will be fixed in Sass 3.0)
- Bundle size slightly over budget (can be optimized if needed)

## Testing

The frontend is ready to connect to the running ASP.NET Core API. Ensure the backend API is running on port 5020.

### Test Users

Use these credentials after setting up the backend:
- **Administrator**: Create via backend seeding
- **Operator**: Create via admin user management
- **Visitor**: Register through `/register` page

## Next Steps

1. Start the ASP.NET Core API backend
2. Run `npm start` in the frontend directory
3. Navigate to `http://localhost:4200`
4. Register a visitor account or login with admin credentials
5. Explore all features based on your role

## Notes

- All components use standalone architecture (no NgModules)
- Lazy loading configured for all feature routes
- JWT tokens stored in localStorage
- All forms include validation and error handling
- Comprehensive error messages for failed operations
