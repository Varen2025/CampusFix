# Understand and run CampusFix in Visual Studio

CampusFix now runs from Visual Studio's green Run button. The solution includes a **docker-compose** project and a **CampusFix in Docker** launch profile. Keep coding in Visual Studio; Docker handles the running environment.

**Verified on 8 October 2026:** Visual Studio's green Run button/F5 launched the Docker profile successfully. The web and API containers ran, PostgreSQL was healthy, and signing in through the MVC website displayed the student's saved resolved report. API sign-in also passed. Visual Studio's red Stop button stopped all three containers in this configured F5 session and kept the database volume. The earlier Docker checks covered registration, creating a report, staff updates, permission restrictions and persistence after restarting. Automatic private-settings creation was checked in a fresh copy, and a second setup preserved the settings. GitHub, live hosting and the remaining assignment evidence still need work.

## 1. What the “Container” option means

At the top of Visual Studio, the dropdown beside the green arrow tells Visual Studio **how to start the project**. A normal project profile starts an app directly on Windows. A Container profile asks Visual Studio's container tools to build and run the app through Docker. Docker Desktop must be available for that to work.

Your earlier project had one containerized app. CampusFix starts three related services, so its dropdown uses **CampusFix in Docker**, a Docker Compose profile. Compose is the tool that arranges these services together. Select the supplied profile rather than adding new container templates to the web project: its configuration already includes the API and database. [Microsoft's Compose tutorial](https://learn.microsoft.com/en-us/visualstudio/containers/tutorial-multicontainer)

## 2. What CampusFix actually does

CampusFix is a maintenance reporting website for a campus. A student reports something needing attention, such as a broken light. Staff see incoming reports and update their status. The student returns to see whether the report is Open, In Progress or Resolved.

Students register and sign in. They see their own reports. Staff have a different role and can see all reports and update them. The API checks these permissions, so hiding a staff button is not the only protection.

The website uses **ASP.NET Core MVC**, which you have used in class. MVC stands for Model, View and Controller. In this project, controllers handle web actions, Razor views produce the pages, and models describe the information the page needs. React is not part of this project.

## 3. Follow one report through the system

The path is **browser → MVC website → API → database**, with the result returning back to the browser.

1. You fill in a report in your browser and select Submit.
2. **CampusFix.Web** receives that form and sends the report information to the API, along with the signed-in user's token.
3. **CampusFix.Api** checks who you are, validates the details and saves the report in PostgreSQL.
4. The API returns the result. The web app shows the updated page in your browser.
5. Staff later update the status through the same web/API/database path. The database keeps that update, and the student can see it next time they open the report.

| Part in the solution | Its job | Separate running Docker container? |
|---|---|---|
| CampusFix.Web | MVC pages, forms, navigation and calls to the API | Yes, the web service |
| CampusFix.Api | Sign-in, permissions, report operations and database access | Yes, the api service |
| PostgreSQL | Stores users and reports | Yes, the db service; it uses an existing PostgreSQL image |
| CampusFix.Contracts | Shared definitions for information sent between web and API | No, a library used by the apps |
| CampusFix.Api.Tests | Automated checks run during testing | No, not a service needed to browse the site |
| docker-compose | Visual Studio's instructions for starting the services together | No additional fourth application container |

PostgreSQL is the database software used by the Docker version. It is already included in this setup; you do not need to add a separate SQL Server project to make CampusFix run.

## 4. What Docker adds

Docker gives each running service its own controlled environment with the files and software it needs. That helps the group use compatible versions instead of manually installing a database and app runtime differently on each computer. A container is a running process in an isolated environment, rather than another copy of your entire Windows desktop. [Docker's container explanation](https://docs.docker.com/get-started/docker-concepts/the-basics/what-is-a-container/)

On Windows, your Docker Desktop installation uses **WSL 2** to support these Linux containers. WSL is Windows Subsystem for Linux. You still use Windows, Visual Studio and your normal browser. Docker manages its Linux environment behind the scenes. [Docker's WSL explanation](https://docs.docker.com/desktop/features/wsl/)

| Docker word | Meaning in CampusFix |
|---|---|
| Dockerfile | A build recipe: prepare the .NET app, copy its required files and choose how it starts. There is one for Web and one for Api. |
| Image | The resulting app package. Building an image prepares a version of the app to run. |
| Container | A running instance of an image. Web, Api and PostgreSQL each run in one. |
| Docker Compose | Reads the shared service configuration and arranges the three containers and their connections. |
| Docker Desktop | The Windows app that provides the engine and a screen for viewing, starting and stopping containers. |
| Volume | Storage managed by Docker, outside the database container's disposable files. The PostgreSQL volume keeps users and reports. |

The image contains an application and its runtime files; it is not the application currently running. A container is the running version. You can leave images saved on disk while stopping the running containers. [Docker's image explanation](https://docs.docker.com/get-started/docker-concepts/the-basics/what-is-an-image/)

Compose also creates a private network where the web app can reach the API and the API can reach the database by their service names. Our Dockerfiles build the two .NET services; PostgreSQL uses the official database image. [Docker's Compose explanation](https://docs.docker.com/get-started/docker-concepts/the-basics/what-is-docker-compose/)

## 5. The files you see

| File or folder | What you use it for |
|---|---|
| CampusFix.sln | Open this solution in Visual Studio. It organizes the projects. |
| src/CampusFix.Web | Edit the pages, MVC controllers and web styling here. |
| src/CampusFix.Api | Edit API operations, authentication and database handling here. |
| src/CampusFix.Contracts | Shared request and response types. |
| tests | Automated checks. |
| Dockerfile in each app | Docker's recipe for that app. |
| docker-compose.yml | Which services to run, how they connect, ports and persistent storage. |
| docker-compose.dcproj | Makes Compose part of the Visual Studio solution and configures its startup. |
| launchSettings.json at the root | Defines the **CampusFix in Docker** toolbar profile. |
| .env | Your computer's private passwords and login-token key. Keep it private. |
| .env.example | A safe template showing the setting names, without your private generated values. |
| .gitignore | Keeps settings, local databases and temporary build files out of Git history. |
| .github/workflows | The prepared automated pipeline for GitHub. It still needs verification in your actual repository. |

You do not need to understand or edit every file to start the app. Begin with the solution, then learn the part you are contributing to.

## 6. Start it using the green button

1. Open **Docker Desktop** from the Start menu. Wait for the engine to be running. The project uses Linux containers. Docker is already installed on the checked computer, so repeated installation or Windows feature changes are unnecessary.
2. In Visual Studio choose **File > Open > Project/Solution**. Open **CampusFix.sln** in the folder directly containing docker-compose.yml and src. Use this supplied solution.
3. In **Solution Explorer**, right-click the project named **docker-compose** and select **Set as Startup Project**. Its name normally becomes bold.
4. At the top toolbar, select **CampusFix in Docker** beside the green Run arrow.
5. Click the green arrow, or press **F5**. Visual Studio builds the app images, asks Docker to start the services and attaches its debugger to the web/API services. **Ctrl+F5** runs without the debugger. Compose is the single startup project even though it starts several services. [Microsoft's startup guidance](https://learn.microsoft.com/en-us/visualstudio/ide/how-to-set-multiple-startup-projects)
6. On a fresh copy, Visual Studio generates a private `.env` automatically when it prepares the project. Your current `.env` is already present and is preserved. If setup reports missing settings, **Setup-Docker.cmd** remains an optional fallback; then try Run again.
7. Wait for the build and startup to finish. The first run can download .NET, PostgreSQL and Microsoft's container debugging tools and take several minutes. The first debugging-tools download on this computer has been completed. **View > Output** shows progress if the browser has not opened yet.
8. The website opens at **http://localhost:5080**. If needed, open that address yourself after startup completes. Register a fictional student and try submitting a report.

If the docker-compose project is unavailable or cannot load, check Visual Studio Installer for the ASP.NET/web development workload and Container Development Tools. If an older Visual Studio cannot support this .NET 10 project, use a compatible version. Do not create a replacement solution to fix a missing workload.

If the profile does not appear immediately, save your files, close the solution and reopen CampusFix.sln, then select docker-compose again. If a startup error remains, keep the exact error message. Visual Studio's Compose Output can include settings from .env, so do not share the full Output log or screenshots of its configuration/password lines. Share the specific error with private values removed.

## 7. Understand the addresses and login details

**localhost** means this computer. **5080** is the port leading to the website. The browser connects to that port, and Docker forwards the request to the web container. **5081** leads to the API. A port lets different services have separate entrances on the same computer. [Docker's port explanation](https://docs.docker.com/get-started/docker-concepts/running-containers/publishing-ports/)

Use **http://localhost:5080** for the site. **http://localhost:5081/swagger** is interactive API documentation for development and demonstration. Your friend cannot open your running app by receiving your localhost address; their localhost points to their own computer. Public hosting is still required for a shared live address.

To find the Docker staff login, in Visual Studio choose **File > Open > File** and open **.env** from the same project folder. **SEED_STAFF_EMAIL** and **SEED_STAFF_PASSWORD** are the staff account values. **POSTGRES_PASSWORD** is the app's database password, not a student login. **JWT_KEY** is a private key used by the API to sign login tokens, not something you type on the sign-in page.

Your friend creates their own settings and student accounts. Keep .env and passwords out of GitHub, messages and presentation screenshots. Changing the seed password does not reset an already-created staff account, and changing POSTGRES_PASSWORD in .env does not change an existing database's password.

## 8. Saved data and the earlier demo

The earlier Windows launcher used a **SQLite file**. The Docker version uses **PostgreSQL in its volume**. They are separate databases, so an account registered in the old Windows demo may not exist in the Docker app. Register in the version you are demonstrating.

Stopping a container stops the service; it does not delete the volume. Starting CampusFix again uses the saved database. Removing the PostgreSQL volume would remove its accounts and reports, so avoid Docker's volume-delete/clean-up actions when you want to keep the data. A volume remains stored on disk even while its container is stopped. [Docker's volume explanation](https://docs.docker.com/engine/storage/volumes/)

The saved-data check is simple: register, submit a report, stop the services, start again and sign back in. The report should remain. This Docker check already passed on the checked computer; live hosting persistence is a separate remaining check.

## 9. Edit code and see the result

1. Edit a file in Visual Studio and save it.
2. Stop the current debugging session with the red square button.
3. Select **CampusFix in Docker** and run again. The configuration uses **Regular** container builds, which rebuild the images from the app source before starting. You may see build output; that is expected. [Microsoft's Compose build settings](https://learn.microsoft.com/en-us/visualstudio/containers/docker-compose-properties)
4. Refresh the website and check the change.

Saving changes alone does not guarantee an already-running container has rebuilt its app. Use Stop and Run for a clear rebuild. Use GitHub Desktop separately to record and upload the change; Docker does not make Git commits.

## 10. Stop it before gaming

1. In Visual Studio, click the **red Stop button** if a debugging session is active. In the configured F5 profile checked on this Visual Studio 2026 installation, that stopped **web**, **api** and **db**.
2. Open **Docker Desktop > Containers**. Find the group named **campusfix**. Expand it if necessary.
3. Confirm **web**, **api** and **db** show as **Stopped/Exited**. If all three are already stopped, you do not need another Stop action. If anything remains running, use the group's **Stop** action, or stop the remaining services individually. This is the fallback when using another launcher, running without debugging with Ctrl+F5, or seeing a running service after Stop. Wait until all three show as stopped/exited.
4. When you also want Docker Desktop closed, right-click its taskbar icon beside the clock and choose **Quit Docker Desktop**. The icon may be under the arrow for hidden icons.

The stored images and database volume can remain on disk. They are needed again next time but are not running app services. Closing your browser only closes the page; it does not stop containers. Docker's Containers screen is where you can check their state. [Docker's container stop instructions](https://docs.docker.com/get-started/docker-concepts/the-basics/what-is-a-container/)

Next time, open Docker Desktop, wait for the engine, open the solution and use the same green Run profile. You do not need to recreate the project or delete your database.

## 11. What comes next for the assignment

Show your friend the screenshot or share the supplied clean ZIP for a local preview. Then agree on the project and create **one shared public GitHub repository**. Everyone clones it, works on their own task branches, commits meaningful work, opens PRs and reviews another member's changes. **FRIEND_AND_GITHUB.md** explains those steps using GitHub Desktop.

After that, verify the GitHub pipeline and security checks, match the lecturer's unit-test/Postman requirement, deploy the separate web/API services with a persistent hosted database, verify automatic deployment and gather credible research. Add the real names, contribution links, evidence and live URL to the README. Finally make the PowerPoint and rehearse the demonstration.

The app running on your computer is a substantial completed part. Group collaboration, hosted results and presentation evidence still have to happen before the complete assignment can be called done.
