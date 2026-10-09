**You can keep doing all your coding in Visual Studio.** Docker runs the same application in containers. You do not need to rewrite CampusFix or learn React.

The app works locally, including in Docker with PostgreSQL, but the complete assignment is not ready for submission. Read the completion checklist below before calling a requirement finished.

**What the app does.** A student registers, signs in, submits a maintenance fault and follows its status. Staff sign in, see incoming reports and update their status. A student cannot see another student's reports or use the staff update action.

**How the parts fit together.** The browser shows pages from the MVC web project. When you submit a report, the MVC server sends it to the API. The API checks permissions and saves it in a database. The response travels back to the page.

| Part | Purpose |
|---|---|
| CampusFix.Web | MVC controllers, Razor pages, forms and the website's appearance. |
| CampusFix.Api | Sign-in, access rules, report operations and database access. |
| CampusFix.Contracts | The shared definitions of the information sent between web and API. This is a library, not another website. |
| CampusFix.Api.Tests | Automated checks of the API. This is not another running service. |
| Database | Stores users and reports after requests finish. |
| docker-compose | The Visual Studio startup project that starts the website, API and PostgreSQL together in Docker. |

The earlier local demo uses a SQLite database file. The verified Docker version uses PostgreSQL. They are separate databases: the earlier accounts, passwords and reports do not transfer into Docker.

**What has been checked.** On 7 October 2026, the solution built without errors or warnings, all five API integration tests passed using SQLite, and the dependency audit found no known vulnerable packages. Browser checks covered registration, a submitted report, staff sign-in and a status update. The report survived restarting both services, and the page was inspected at a phone-sized width.

On 8 October, both Docker images built and all three services started. The website and API/database health checks passed. A student registered and created a report; staff signed in and marked it **Resolved**. API checks blocked requests without sign-in, a student's staff update attempt and another student's access to the report. The account and resolved report survived stopping and restarting Docker containers; browser sign-in displayed the saved report. Swagger also opened. These were separate workflow checks; the five automated integration tests were checked with SQLite. Live hosting and the GitHub pipeline still need verification.

The Visual Studio green **CampusFix in Docker** Run button was also tested on 8 October. Visual Studio showed the app running, the website and API containers started with the database, and student sign-in displayed the previously saved resolved report. Clicking Visual Studio's red Stop button stopped all three containers, and the database volume remained saved. A new dependency audit including the container tooling found no known vulnerable NuGet packages in the four application/test projects.

**Open it in Visual Studio.** Use File > Open > Project/Solution and select CampusFix.sln inside the CampusFix folder. Do not create a replacement project. On the machine checked in this chat, Visual Studio Community 2026 18.10, the ASP.NET web-development workload and .NET SDK 10.0.401 are installed. This supports the project's .NET 10 target. See [Microsoft's compatibility table](https://learn.microsoft.com/en-us/dotnet/core/porting/versioning-sdk-msbuild-vs).

Edit the code in Visual Studio and save your changes. The solution includes a **docker-compose** startup project and a **CampusFix in Docker** Run profile. Visual Studio's green Run button now builds and starts the three containers together. This launch worked in the actual Visual Studio interface; the optional start and stop buttons also passed their earlier checks.

**Docker words in plain language.** A Dockerfile is a build recipe. An image is the resulting application package. A container is a running instance of that package. Docker Compose reads docker-compose.yml and starts the related services together. Our setup has web, api and db services. A volume stores the PostgreSQL data separately from the disposable database container.

**Docker Desktop is now installed.** Its Linux engine has been checked on this computer. You do not need to reinstall it or repeat the Windows feature changes. For a group member's new computer, use the [official Docker Desktop Windows installation page](https://docs.docker.com/desktop/setup/install/windows-install/), select Linux containers and follow any restart instructions. Docker permits free educational use. [Docker licensing](https://docs.docker.com/subscription-billing/desktop-license/)

**Run from Visual Studio.** This everyday workflow was verified on 8 October 2026.

1. Open Docker Desktop and wait for its engine to run. Docker Desktop supplies the environment in which the three containers run; Visual Studio remains your code editor.
2. In Visual Studio, open **CampusFix.sln** from the folder that directly contains docker-compose.yml. In Solution Explorer, right-click **docker-compose** and choose **Set as Startup Project** if it is not already selected. Do not select only CampusFix.Web: that would leave the API and database out of the launch.
3. Select **CampusFix in Docker** beside the green Run button. Click Run or press **F5**. This profile starts PostgreSQL, the API and the MVC website together. Missing private settings are generated; existing .env settings are left unchanged. It also stops only processes recorded by the earlier Windows launcher. The first build can take several minutes, and scrolling build messages are normal. Visual Studio may also download Microsoft's container debugging tools on the first launch; allow time for that download.
4. The profile opens [localhost:5080](http://localhost:5080). Register a fictional student account and submit a report. For staff sign-in, use **File > Open > File** in Visual Studio to open .env in the same project folder. Its SEED_STAFF_EMAIL and SEED_STAFF_PASSWORD are the Docker staff login. Keep them private. Update the report as staff, then sign in as the student and check the result. API documentation is at [localhost:5081/swagger](http://localhost:5081/swagger).
5. After editing code, save, click Visual Studio's red Stop button and click Run again. Visual Studio rebuilds the Docker version with your changes.

**What happens to your data.** When a student submits a form, the MVC website sends it to the API. The API checks who signed in and saves the report in PostgreSQL. The database volume keeps accounts and reports when containers stop or are recreated. Localhost means this computer; Docker does not make the website public automatically.

**Stop everything before gaming.** Click Visual Studio's red **Stop** button. In this configured Visual Studio 2026 Compose run, that stopped the website, API and database during the check. Open **Docker Desktop > Containers** and confirm the three **campusfix** containers are stopped; they may be shown as **Exited**. If they are already stopped, you do not need to click Stop again. If any are still running, for example after a separate launcher or Ctrl+F5, use the **campusfix** group's **Stop** control. Keep the database volume. Finally, right-click the Docker Desktop icon beside the taskbar clock and choose **Quit Docker Desktop** if you want Docker Desktop closed too. The icon may be under the small arrow for hidden icons. Stopping containers and quitting Docker Desktop are separate actions, and both preserve saved data.

**Optional double-click buttons.** Start-Docker.cmd and Stop-Docker.cmd remain available beside CampusFix.sln. They were tested successfully, including from a different working folder. Stop the Visual Studio run first if using them. Start-Docker.cmd opens the installed Docker Desktop if needed, preserves .env, builds and starts all three services, checks both health endpoints and opens the site. Stop-Docker.cmd removes this project's containers while keeping accounts and reports. Closing the start window alone does not stop them. Setup-Docker.cmd only prepares private settings; your current .env is already prepared. The earlier Start CampusFix.cmd route uses its separate Windows/SQLite database.

Continue using **GitHub Desktop** for commits, branches and pushing your repository. The Visual Studio Run workflow does not require terminal commands. CampusFix.App.slnf is a separate selection of the application and test projects for command-line checks and CI; keep opening CampusFix.sln for the full Visual Studio Docker setup.

**What the private settings mean.** POSTGRES_PASSWORD is the API's database password. JWT_KEY is the API's private key for signing login tokens; it is not a password that students type. SEED_STAFF_EMAIL and SEED_STAFF_PASSWORD create the first staff account. .env.example is the safe blank template; .env holds your generated values. Never upload .env, .local or their passwords. The Git ignore file already excludes them.

Do not regenerate database passwords after your PostgreSQL volume has been created. Changing .env alone will not change that existing database password. Changing the staff seed password also does not reset an existing staff account.

**Completion checklist, as of 8 October 2026.** Done means demonstrated, Prepared means files exist but the relevant system has not been verified, and Pending means work remains.

| Item | Status | What remains |
|---|---|---|
| MVC website and .NET API | Done locally | Group members should understand and explain the code. |
| Student/staff workflows and access restrictions | Done locally and in Docker | Repeat the flows on live hosting and learn the demonstration. |
| API integration tests | Done locally | Run the pipeline; match the brief's unit-test/Postman wording before submission. |
| Swagger API documentation | Done locally and in Docker | Verify the hosted endpoint when deployed. |
| SQLite persistence | Done locally | The Windows launcher uses SQLite; Docker's PostgreSQL was checked separately. |
| Docker Desktop installation and Linux engine | Done | Use Linux containers when running the project. |
| Two Dockerfiles, Compose and start/stop buttons | Done on this computer | Both images built; all three services and both buttons worked. |
| Visual Studio Compose Run and Stop | Done on this computer | Run displayed the saved report; red Stop stopped all three services and kept the database volume. |
| PostgreSQL volume and connection | Done in Docker | Account and resolved report survived stopping and restarting containers. Hosted persistence remains separate. |
| .env.example and ignored secrets | Done locally | Private settings are created in the correct folder; keep .env out of screenshots and GitHub. |
| GitHub Actions and security scans | Prepared | Push to your group repository and fix any failed jobs. |
| Automatic deployment hooks | Prepared | Configure both services and secret hooks, then prove a merge deploys successfully. |
| Public group repository and review history | Pending | Use real individual accounts, branches, commits and reviews. |
| Public hosting and hosted database | Pending | Create permitted free services and verify live persistence. |
| Research | Pending | Use credible sources and/or actual conversations to justify the scenario. |
| README | Partly complete | Add research, names, real contributions and the live URL. |
| PowerPoint | Pending | An outline exists; no .pptx has been created. |
| Group confirmation and rehearsal | Pending | Confirm who is registered and practise the actual demonstration. |

The [lecturer's repository](https://github.com/SMahabeer07/ICE-Task-CLDV6212) remains the authority. A realistic business scenario is allowed; interviews are a suggestion, not the only acceptable research method. All registered members must participate, so ask the lecturer how an absence should be handled.

**The remaining work in practical order.** Establish the public group repository, reviewed changes and passing CI. Add the unit-test or Postman evidence required by the brief; the existing five checks are integration tests. Deploy the two application services with a hosted database, verify persistence and prove automatic deployment after a merge. Collect research and fill the README while group members work on these steps. Finally create the PowerPoint from real screenshots and results and rehearse. Docker now works on this computer, but localhost does not prove public hosting or individual group contributions.

The supplied deadline is 9 October 2026 at 23:59 South African time. Aim to upload the PowerPoint and repository link by 21:00. Your pasted brief permits the presentation later during class.
