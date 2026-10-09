# Share CampusFix, then work together with GitHub Desktop

First show your friend the app. After the group agrees on it, use one shared GitHub repository. You can code in Visual Studio and use GitHub Desktop for the teamwork. You do not need to type Git commands.

The app works locally, but the complete assignment still needs real group contributions, a verified GitHub pipeline, public hosting, research and the PowerPoint. A ZIP gives your friend the code; it does not create contribution history.

## 1. Show your friend before setting up GitHub

Send the supplied **CampusFix.zip** and a screenshot of the website through your usual messaging app. The supplied ZIP contains the source and setup files, without your private settings or local accounts/reports. Do not send a ZIP you make from the entire working folder: that could include your `.env`, private passwords or local database.

The address `http://localhost:5080` means **the computer opening that address**. Sending that address to your friend will not let them see your computer's app. For a first opinion, a screenshot or screen-sharing call is quickest. To try the app themselves, they must run their own copy using the steps below. Later, the hosted website will have a public address that everyone can open.

Your friend should:

1. Download the supplied ZIP, right-click it and choose **Extract All**. Choose a fresh folder. Do not extract it inside another CampusFix project.
2. Find the folder directly containing **CampusFix.sln**, **docker-compose.yml**, **src** and **docs**. This is the project folder.
3. Install Docker Desktop if needed, open it and wait until its engine is running. Use Linux containers. Follow Docker's restart instructions if Windows features need enabling. See [Docker's Windows installation guide](https://docs.docker.com/desktop/setup/install/windows-install/).
4. Use a Visual Studio version supporting .NET 10 and the ASP.NET/web development workload. The checked computer here has Visual Studio Community 2026. The project also needs Visual Studio's container tooling for the Compose startup option.
5. Visual Studio creates their private **.env** automatically during the first project setup, with new settings for their computer. Existing settings are preserved, and .env is ignored by Git. Automatic creation and preservation were checked in a fresh copy. If a missing-settings error appears, **Setup-Docker.cmd** is an optional one-time fallback.
6. Open **CampusFix.sln** in Visual Studio. In Solution Explorer, right-click **docker-compose** and choose **Set as Startup Project**. Select **CampusFix in Docker** beside the green Run button. Click that button, or press **F5**. **Ctrl+F5** starts without the debugger. Green Run/F5 was verified on 8 October 2026. On a friend's first run, Microsoft's container debugging tools and app/database files may need downloading; allow several minutes. The included Visual Studio guide explains this setup.
7. Register a fictional student, report a fault and try the staff workflow. Their `.env` holds their staff login. Their accounts and reports belong to their own database; your saved demo reports are not in the ZIP.

When finished with an F5 session, click Visual Studio's **red Stop button**. It stopped all three containers in the configured Visual Studio 2026 session checked here and preserved the database volume. Open **Docker Desktop > Containers > campusfix** and confirm web, api and db show **Stopped/Exited**. If they are already stopped, no additional Stop action is needed. If anything remains running, or you used another launcher/Ctrl+F5, use the group's Stop action as a fallback. You can then **Quit Docker Desktop** from its taskbar icon before gaming.

Once the group repository exists, everyone should clone that repository for actual teamwork. Keep the preview copy as a separate demo, so changes are not accidentally made in a folder GitHub Desktop does not track.

## 2. Understand the GitHub words

| Word | What it means here |
|---|---|
| Repository or repo | The project files plus their recorded change history. |
| Clone | Download a working copy with its history, connected to the shared GitHub repository. |
| main | The shared version the group has accepted. |
| Branch | Your separate line of work inside the same repository. |
| Commit | Record a set of changes on your computer with a short explanation. Saving a file in Visual Studio does not create a commit. |
| Push | Upload your commits to GitHub. |
| Fetch | Check whether GitHub has new commits. |
| Pull | Bring those new commits into your current local branch. |
| Pull request or PR | Propose merging a branch into main and ask for review. |
| Review | Another member examines the changes, tests what matters and leaves feedback or approval. |
| Merge | Add the approved branch changes to main. |
| CI or checks | Automated build, test and security work run by GitHub Actions. |

## 3. Create one group repository when you are ready

One person owns the shared repository and invites the others. Everyone uses their own account. To keep application changes on branches and PRs from the beginning, create a small README-only baseline first, then import CampusFix on a feature branch. This initializes main; it does not claim completed group work.

1. Sign in to **GitHub in your browser**. Choose **+ > New repository**.
2. Choose the owner and a descriptive group repository name. Select **Public** to meet the lecturer's requirement.
3. Select **Add a README file** and create the repository. The supplied CampusFix README will replace this small starting README through the first PR. Leave new Git-ignore/license templates unselected; the supplied project already includes its ignore file. [GitHub's repository creation guide](https://docs.github.com/en/repositories/creating-and-managing-repositories/creating-a-new-repository)
4. Invite your friends using the next section. Clone this repository into a **fresh folder** using GitHub Desktop. Keep the current working project separately until the import is complete.
5. In Desktop create **feature/initial-campusfix** based on main **before copying the app files**.
6. Extract the supplied clean ZIP into a separate temporary folder. Copy the contents of its project folder into the cloned repository's root: CampusFix.sln, src, tests, docs, .github, .gitignore, .dockerignore and the other supplied project files. Replace the small initial README with the supplied one. Do not copy the entire outer CampusFix folder as another folder inside the clone. Keep the clone's existing .git folder.
7. Check Desktop's **Changes**. CampusFix.sln and src should be at the repository root. `.env`, `.local`, `.vs`, bin, obj and the accidental nested copy must not be committed. `.env.example` is safe and should be included. Stop before uploading if private settings appear.
8. Commit to **feature/initial-campusfix**, click **Publish branch**, then open a PR to main. Ask a friend to review the starting project. The PR should explain that this is the shared starting code and state which checks were actually performed. Fix failed checks or review concerns, then merge after approval. The full PR workflow is in section 6.

Configure main protection using section 5, then require real subsequent work through reviewed PRs. Do not invent an exception permitting ongoing direct application commits to main. **No group repository has been created or published by these instructions alone.**

## 4. Invite your friends and let them clone it

On the repository's GitHub page, the owner selects **Settings > Collaborators > Add people**. Search each friend's exact GitHub username and send the invitation. Each friend accepts it using their own account. [GitHub's collaborator instructions](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/repository-access-and-collaboration/inviting-collaborators-to-a-personal-repository)

Then each friend opens the shared repository on GitHub, selects **Code > Open with GitHub Desktop**, chooses a fresh local folder and clicks **Clone**. Before the first app PR is merged, the app is on **feature/initial-campusfix**: select that published branch in Desktop to try it. After it is merged, select main and fetch/pull. They open the solution from the clone and use their own private settings. They do not publish a separate group repository. [GitHub's cloning instructions](https://docs.github.com/en/desktop/adding-and-cloning-repositories/cloning-a-repository-from-github-to-github-desktop)

If you already ran the preview on **this same computer**, stop its containers first and privately copy **your own existing .env** from the preview project into your cloned project **before opening the cloned solution in Visual Studio**. Visual Studio may prepare settings while opening the solution, before you press Run. Do this locally only; do not send that file or commit it. Compose uses the same campusfix database volume on that computer, so generating a new database password in the clone would not match the existing database. A friend on a different computer uses their own generated settings. The source import itself still uses only the clean ZIP.

## 5. Protect main before ongoing group changes

The owner opens **Settings > Branches > Add classic branch protection rule**. Use **main** as the branch name pattern. Require a pull request before merging and **one approving review**. After the workflow has run on the first app PR, require its build/test and security checks to pass, and require branches to be up to date. Select the real check names shown by GitHub. Keep force pushes and branch deletions disabled; enable the option preventing bypass if available. Save the rule. Failed scans/checks need fixing; do not bypass the rules just to merge. [GitHub's branch-protection instructions](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/managing-a-branch-protection-rule)

Two members can review each other's PRs. Three members can rotate reviewers. Each member should both make a meaningful change and review another member's work. An initial upload alone does not demonstrate that collaboration.

## 6. Make one real contribution on your own branch

Agree on small tasks before editing so two people are not rewriting the same file at once. Examples to discuss are:

| Area | A useful contribution |
|---|---|
| Website | Improve keyboard navigation, form guidance or phone layout; check the affected pages. |
| API/testing | Add a targeted unit test or Postman check for a permission or validation rule; demonstrate the result. |
| Research/docs | Gather actual credible research, justify the scenario, document findings and verify the setup steps. |
| Delivery | Configure and verify the pipeline/hosting, then document real results and live URLs. |

Choose tasks the member can understand and explain. Do not make empty commits, change names to claim existing code, share account credentials or invent author/review history. The generated starting project should be described honestly. If the lecturer expects a particular kind of coding contribution from everyone, choose tasks that meet it.

For each task:

1. In GitHub Desktop select **Current branch > main**. Click **Fetch origin**, then **Pull origin** if offered. Start with a clean Changes list.
2. Select **Current branch > New branch**, give it a clear task name such as **feature/report-accessibility**, base it on main and create it. Each task gets its own branch. [GitHub's branch guide](https://docs.github.com/en/desktop/making-changes-in-a-branch/managing-branches-in-github-desktop)
3. Edit this clone's files in Visual Studio, save and run the app. Check the behavior you changed.
4. Return to GitHub Desktop. Read the Changes list and diff, then commit with a description of what you changed and why. Make sure the button names your feature branch.
5. Click **Publish branch** on its first upload, or **Push origin** for later commits.
6. Click **Preview Pull Request**, check that the base is main, then **Create Pull Request**. Finish it in the browser with a clear title, what changed and how you checked it. Request another group member's review. [GitHub's PR guide](https://docs.github.com/en/desktop/working-with-your-remote-repository-on-github-or-github-enterprise/creating-an-issue-or-pull-request-from-github-desktop)
7. The reviewer reads **Files changed**, tries the relevant behavior and leaves useful feedback. They can submit an **Approve** review after concerns are addressed. The author cannot approve their own PR.
8. If changes are requested, edit, commit and push to that same branch. The existing PR updates automatically. After approval, passing required checks and no unresolved conflicts, use **Merge pull request** on GitHub.
9. Everyone returns to main in Desktop, fetches and pulls. They now have the accepted change. Delete the merged feature branch when ready, then create a fresh branch for the next task.

## 7. If changes conflict

A conflict means two changes overlap and Git needs someone to choose the final contents. Commit or preserve your current work before changing branches. Use Desktop's branch update/merge-from-main option and open conflicted files in Visual Studio. Its merge editor shows both versions. Agree with the other author on the final result, save it, check the app and complete Desktop's merge, then push your branch. Do not fix a conflict by force-pushing main or deleting someone else's work.

## 8. Keep evidence as you work

Record each member's actual names/student numbers, tasks, commits and PR links in the README. Capture a real review and passing checks for the presentation. Keep private settings out of screenshots. Record research sources and live hosting results when they exist. The assignment is complete only when those remaining requirements have been demonstrated and the PowerPoint and repository link are submitted.
