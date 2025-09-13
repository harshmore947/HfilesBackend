# Medical Record Dashboard API

This API provides endpoints for user authentication, profile management, and medical file uploads using ASP.NET Core, PostgreSQL, and Azure Blob Storage.

## Base URL

```
http://localhost:5027
```

## Authentication

All endpoints except registration require session-based authentication. Login first to establish a session.

## Endpoints

### Authentication

#### Register User

- **POST** `/api/auth/register`
- **Body (JSON):**
  ```json
  {
    "FullName": "John Doe",
    "Email": "john@example.com",
    "phone": "1234567890",
    "Password": "password123",
    "Gender": "Male",
    "ProfileImageUrl": null
  }
  ```
- **Response:** `200 OK` with `{"message": "Registered"}`

#### Login User

- **POST** `/api/auth/login`
- **Body (JSON):**
  ```json
  {
    "Email": "john@example.com",
    "Password": "password123"
  }
  ```
- **Response:** `200 OK` with `{"message": "Logged in"}` (sets session cookie)

#### Logout User

- **POST** `/api/auth/logout`
- **Response:** `200 OK` with `{"message": "Logged out"}`

### Profile Management

#### Get Profile

- **GET** `/api/profile`
- **Response:**
  ```json
  {
    "id": 1,
    "fullName": "John Doe",
    "email": "john@example.com",
    "phone": "1234567890",
    "gender": "Male",
    "profileImageUrl": "https://via.placeholder.com/150x150/cccccc/666666?text=User"
  }
  ```

#### Update Profile

- **PUT** `/api/profile`
- **Body (JSON):**
  ```json
  {
    "FullName": "Updated Name",
    "Phone": "9876543210",
    "Gender": "Female",
    "ProfileImageUrl": "https://example.com/new-image.jpg"
  }
  ```
- **Response:** `200 OK` with `{"message": "Profile updated successfully"}`

#### Upload Profile Image

- **POST** `/api/profile/upload-profile-image`
- **Body (form-data):**
  - `file`: Image file (jpg, jpeg, png, gif)
- **Response:**
  ```json
  {
    "message": "Profile image uploaded",
    "imageUrl": "https://youraccount.blob.core.windows.net/hfiles/profiles/..."
  }
  ```

### Medical Files

#### Upload Medical File

- **POST** `/api/medical-files/upload`
- **Body (form-data):**
  - `fileType`: "Lab Report", "Prescription", "X-Ray", "Blood Report", "MRI Scan", "CT Scan"
  - `fileName`: Custom name (e.g., "Ankit's Lab Report for Typhoid")
  - `file`: PDF or image file
- **Response:** `200 OK` with `{"message": "File uploaded successfully", "fileId": 1}`

#### Get Medical Files

- **GET** `/api/medical-files`
- **Response:**
  ```json
  [
    {
      "id": 1,
      "fileName": "My Report",
      "fileType": "Lab Report",
      "blobUrl": "https://youraccount.blob.core.windows.net/hfiles/medical/...",
      "uploadedAt": "2025-09-13T00:00:00Z"
    }
  ]
  ```

#### Delete Medical File

- **DELETE** `/api/medical-files/{id}`
- **Response:** `200 OK` with `{"message": "File deleted successfully"}`

## Error Responses

- `401 Unauthorized`: Not logged in
- `400 Bad Request`: Invalid input or missing fields
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

## Setup

1. Configure PostgreSQL and Azure Blob Storage in `appsettings.json`.
2. Run migrations: `dotnet ef database update`.
3. Start the app: `dotnet run`.

## Technologies

- ASP.NET Core 9.0
- Entity Framework Core
- PostgreSQL
- Azure Blob Storage
- BCrypt for password hashing
