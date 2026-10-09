# Group work and presentation evidence

Use this as a plan, then replace placeholders with your own evidence. Generated code is not evidence of completed research, individual contributions or live deployment.

## Before coding further

- Confirm whether the registered group has two or three members and record names/student numbers in the README.
- Submission is due **9 October 2026 at 23:59 South African time**, as supplied by the group. Aim to upload by 21:00. Confirm the presentation time limit with the lecturer; the supplied brief allows presentation after submission during class.
- Ask the lecturer how a registered member's absence must be handled. The repository requires all members to participate; a two-person fallback does not waive that requirement.
- Support the problem with credible sources and/or conversations with intended users. The lecturer permits a realistic business scenario; interviews are optional. If interviewing people, record dates, methods and anonymised findings with consent.
- Revise the app scope based on what you learn. Do not claim that the proposed reporting problem is already proven.

## Share the work

| Area | Two members | Three members |
|---|---|---|
| Interface and student workflow | Member 1 | Member 1 |
| API, database and permissions | Member 2 | Member 2 |
| Testing, containers and pipeline | Shared | Member 3 leads; everyone learns it |
| Research and presentation | Both | All three |

Both reliable presenters should be able to start the app and deliver the whole demonstration. Keep the potentially absent member involved in meaningful work, while ensuring another member can explain and run it.

## Build real GitHub history

- Create your group's repository and add these files at its root.
- Each person works under their own GitHub account on a named feature branch, makes meaningful changes and opens their own pull request.
- Every member reviews at least one other member's PR. Record useful feedback and the actual response to it.
- Protect `main`, require PR review and passing CI checks, and merge your reviewed work.
- Link the actual PRs in the README. Do not invent earlier commits, reviews or contributor identities.

## Verify and collect evidence

- Research findings and a justified, limited scope.
- Working student and staff journeys, validation and permission boundaries.
- Both Dockerfiles and a successful `docker compose up --build` with three services.
- A persistence demonstration after stopping and restarting containers without deleting the volume.
- Test results, passing CI, dependency/image scan output and a completed live deployment.
- Public URL, completed member table and actual contribution links in the README.
- A `.pptx` and repository link ready for submission.

## Suggested eight-slide story

1. **CampusFix and the group:** names, roles and one sentence about the app.
2. **The problem:** your real research and intended users.
3. **The proposed solution:** scope and key user journeys.
4. **Architecture:** browser, MVC web app, API and PostgreSQL.
5. **Live demo:** student reports a fault; staff updates it; student sees progress.
6. **Docker and persistence:** start the services and show saved data after restart.
7. **Delivery and teamwork:** actual tests, scans, pipeline, deployment and PR reviews.
8. **Results and limitations:** what your evidence demonstrates, remaining limitations and next steps.

For two presenters, member 1 can lead slides 1–4 and the student demo; member 2 can lead the staff demo and slides 6–8. For three, member 3 can lead slides 6–8. Rehearse handovers and prepare screenshots of your own working app in case the network fails. Screenshots support a fallback; they do not substitute for proving the app works.

Check the [assignment repository](https://github.com/SMahabeer07/ICE-Task-CLDV6212) before submission. It is the authority for requirements; lecturer clarification is needed for absence and presentation timing.
