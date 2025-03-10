# CMAPTask Project

## Overview

**CMAPTask** is a timesheet management application developed with **.NET Core 8** following the **Clean Architecture** pattern. The application is designed to help users log and manage their work hours across multiple projects. It supports viewing timesheet entries, validating input, and exporting data to CSV format.

### Created by: Michael Silva

## Project Structure

The project is organized using **Clean Architecture** with the following layers:

- **CMAPTask.Application**: Contains the application logic, including services and business rules. This layer defines the interfaces for interacting with the data and application logic. The **TimesheetDto** used for transferring data is located here.
- **CMAPTask.Domain**: Defines the core domain models and interfaces that are independent of the application framework or database. This includes entities like `Timesheet`.
- **CMAPTask.Infrastructure**: Provides implementation for data access, services, and external dependencies. It contains the implementation of the repository pattern, database context, and interactions with SQLite.
- **CMAPTask.web**: The presentation layer, built using **ASP.NET Core MVC**, that exposes the user interface for interacting with the application (e.g., logging timesheets, downloading reports, etc.).
- **CMAPTask.Tests**: Contains unit tests for the application, using **XUnit** as the testing framework to ensure the correctness of application logic.

## Key Features

- **Log Time Entries**: Users can log hours worked on various projects, providing descriptions and hours worked.
- **View Timesheets**: Users can view a list of timesheet entries.
- **Export to CSV**: Timesheet data can be exported into a CSV format for reporting purposes.
- **Validation**: Input validation ensures only valid timesheet data is processed.

## Database

The application uses **SQLite** as the database. The SQLite database file is named `CmapTask.db` and is located in the **root directory** of the `CMAPTask.web` project.

### Database Configuration
- **SQLite** is configured using **Entity Framework Core**.
- The database is automatically created upon running the application, and migrations are supported if needed.

## Setup Instructions

### Prerequisites
- .NET 8 SDK or later.
- SQLite database support.

### Steps to Run the Application

1. Clone the repository:
   ```bash
   git clone <https://github.com/michaelfco/CMAP.git>
