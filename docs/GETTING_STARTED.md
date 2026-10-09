# Getting started on Windows

## Open the project

1. Keep the entire `CampusFix` folder together. In File Explorer, use the folder that directly contains `CampusFix.sln` and `docker-compose.yml`. You do not need to open a terminal.
2. For coding and the Windows launcher, install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) if needed. SDK 10.0.401 was already found on the computer checked in this chat.
3. In Visual Studio, choose **File > Open > Project/Solution** and select `CampusFix.sln`. Visual Studio Community 2026 18.10 and the **ASP.NET and web development** workload are already installed on the checked computer. Microsoft documents .NET 10 targeting support in [Visual Studio 18.0+](https://learn.microsoft.com/en-us/dotnet/core/porting/versioning-sdk-msbuild-vs).
4. Use the **docker-compose** startup project and **CampusFix in Docker** profile below. The full solution includes all three services. `CampusFix.App.slnf` is reserved for application-only command-line builds and CI.

## Run from Visual Studio with Docker and PostgreSQL

The Docker app was verified on this computer on 8 October 2026. Both application images built, all three services started, and the website and API/database health checks passed. Student registration, report creation, staff sign-in and a **Resolved** update worked. Permission checks blocked unauthorised access. The account and resolved report survived stopping and restarting the containers; browser sign-in also worked. The private `.env` is in the correct project folder.

Visual Studio's **CampusFix in Docker** green Run button was also verified on 8 October. The website and API containers started with the healthy database, and student sign-in displayed the previously saved resolved report. The red Stop button stopped all three containers while keeping the database volume. The optional start/stop buttons passed their earlier checks too.

1. Open **Docker Desktop**, use Linux containers and wait for its engine to run.
2. In Visual Studio's Solution Explorer, right-click **docker-compose** and choose **Set as Startup Project** if it is not already selected. Select **CampusFix in Docker** beside the green Run button.
3. Click the green Run button or press **F5**. The profile builds the images, starts the PostgreSQL database, API and website together, and opens the website. Missing `.env` settings are generated; existing settings stay unchanged. Only processes recorded by the earlier Windows launcher are stopped automatically. The first build and download of Microsoft's container debugging tools can take several minutes.
4. Register a new fictional student account. For staff sign-in, use **File > Open > File** in Visual Studio to open `.env` from this same project folder. Use `SEED_STAFF_EMAIL` and `SEED_STAFF_PASSWORD`. Keep this file and its contents private.
5. After editing code, save, click Visual Studio's red Stop button and click Run again to rebuild the Docker version with your changes.

On a new computer, install [Docker Desktop for Windows](https://docs.docker.com/desktop/setup/install/windows-install/) with Linux containers first. **Setup-Docker.cmd** can prepare `.env` separately and preserves an existing file. The Docker staff account and database are separate from those used by **Start CampusFix.cmd**.

Once the services are ready, open:

- Website: <http://localhost:5080>
- API documentation: <http://localhost:5081/swagger>
- API/database health: <http://localhost:5081/health>

Your browser shows the MVC website. The website sends requests to the API, and the API saves accounts and reports in PostgreSQL. The database's named volume (normally `campusfix_postgres_db`) keeps those records when containers stop or are recreated. Localhost means this computer; it is not a public hosting address. Inside Docker, the web server calls `api:8080` and the API connects to `db:5432`; PostgreSQL is not directly exposed to the computer's network.

## Stop the project and close Docker Desktop

1. Click Visual Studio's red **Stop** button. This stopped all three services in the configured Visual Studio 2026 Compose run checked on this computer.
2. Open **Docker Desktop > Containers** and confirm all three **campusfix** containers are stopped, possibly shown as **Exited**. If they are already stopped, no further Stop click is needed. If any remain running, for example after a separate launcher or Ctrl+F5, use the **campusfix** group's **Stop** control. The saved database volume remains in place.
3. To close Docker Desktop before gaming, right-click its icon beside the taskbar clock and choose **Quit Docker Desktop**. The icon may be under the small arrow for hidden icons. Stopping containers and quitting Docker Desktop are separate actions.

Do not delete the database volume. Closing a launcher window or browser does not stop the containers.

## Optional double-click launchers

**Start-Docker.cmd** and **Stop-Docker.cmd** remain beside `CampusFix.sln` and have been tested. Stop the Visual Studio run first if using them. The start button opens installed Docker Desktop if needed, preserves settings, builds and starts the three services, checks both health endpoints and opens the website. The stop button removes only this project's containers and keeps the PostgreSQL database. Neither requires terminal commands.

**Start CampusFix.cmd** and **Stop-CampusFix.cmd** run the earlier Windows/SQLite development version. Its data and generated staff login are kept under `.local`, separate from Docker. Closing its start window does not stop those services. Use the PostgreSQL Docker version for the assignment's container demonstration. You can continue using GitHub Desktop for repository work.

## Try the main workflow

1. Register a student account and sign in.
2. Submit a fault with a clear title, location, category and description.
3. Open its detail page and check its initial status.
4. Sign out, sign in as the staff account and find that report.
5. Change the status to **In progress**, then **Resolved**.
6. Return to the student account and confirm the updated status.
7. Register another student and confirm the first student's report is not visible.
8. Restart the stack without deleting its data and confirm the report remains.

Use fictional demonstration records, not classmates' private information.

## Common problems

| Symptom | Next step |
|---|---|
| `dotnet` is not found | Install the SDK, then reopen Visual Studio and the launcher. |
| Port 5080 or 5081 is busy | Stop the other CampusFix run. If Visual Studio is debugging it, click its red Stop button. |
| Visual Studio offers only the web project | Right-click **docker-compose** and choose **Set as Startup Project**, then select **CampusFix in Docker**. |
| Compose says an environment variable is missing | Confirm `.env` is beside `CampusFix.sln`. **Setup-Docker.cmd** prepares it if missing; do not replace an existing file. |
| API cannot connect to PostgreSQL after changing `.env` | An existing volume keeps its original database password. Restore the original value or intentionally migrate/reset the database. |
| Staff sign-in fails | Use credentials for the route you started. Seed settings do not overwrite an existing account's password or role. |
| The site opens but cannot load reports | Check API `/health` and `.local` logs or `docker compose logs api`. |
| Docker reports no engine/daemon | Open Docker Desktop, wait for its engine to run, then try Visual Studio Run again. |

The optional start and stop buttons were tested successfully, including starting from a different working folder and retaining PostgreSQL data. Visual Studio's green Run workflow was also verified. Cloud deployment and the GitHub pipeline remain separate work. Repeat the workflow to learn the demonstration and keep screenshots of actual results for your presentation.
