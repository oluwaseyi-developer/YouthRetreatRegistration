# Youth Retreat Registration — C&S Ayo Ni O, Ikotun-Egbe

A Blazor Server registration app for the Youth Fellowship retreat.

## Features
- 📝 Registration form with validation & duplicate detection
- 📊 Admin dashboard with stats, paginated table, and delete
- 📖 Scrolling prayer scriptures
- 💾 SQLite database for persistent storage
- 🐳 Dockerized for easy deployment

## Run Locally

```bash
cd YouthRetreatRegistration
dotnet run
```

Open `http://localhost:5217`

## Run with Docker

```bash
# Build and run
docker compose up -d

# Or build manually
docker build -t youth-retreat .
docker run -p 8080:8080 -v youth-data:/app/Data youth-retreat
```

Open `http://localhost:8080`

## Deploy from GitHub

Every push to `main` builds a Docker image and pushes it to **GitHub Container Registry**.

### Pull and run anywhere:
```bash
docker pull ghcr.io/<your-username>/youthretreatregistration:latest
docker run -d -p 8080:8080 -v youth-data:/app/Data ghcr.io/<your-username>/youthretreatregistration:latest
```

### Deploy to a VPS (e.g., DigitalOcean, Azure VM, Railway):
1. SSH into your server
2. Install Docker
3. Run the `docker pull` and `docker run` commands above
4. The SQLite database is stored in a Docker volume (`youth-data`) so data survives container restarts

## Admin Login
- **Username:** `YouthAdmin`
- **Password:** `Ikotunegbe`
