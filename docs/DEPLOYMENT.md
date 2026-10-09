# Deployment setup still to complete

**No services have been created and no live URL exists yet.** The workflow contains optional Render deployment hooks, not a deployed environment. It does not create accounts, databases or billable resources.

The assignment requires genuinely free services and disallows payment/card details and trials that convert to paid plans. Before choosing any provider, check its current official pricing, account requirements, container support and database limits. If the current offer cannot meet those rules, choose another provider and adapt the workflow. No free-plan eligibility is assumed here.

## Prepare the hosting environment

If Render meets the rules for your account when you deploy:

1. Create two Docker web services connected to your own project repository and `main` branch. Use the repository root as the Docker build context. Set the API Dockerfile to `src/CampusFix.Api/Dockerfile` and the web Dockerfile to `src/CampusFix.Web/Dockerfile`.
2. Provide a persistent PostgreSQL database. Use the provider's connection details and required TLS settings. Local Compose volumes do not transfer to hosted services.
3. Configure the API environment with `ASPNETCORE_ENVIRONMENT=Production`, `ASPNETCORE_HTTP_PORTS=8080`, `DbProvider=Postgres`, `ConnectionStrings__CampusFix`, and a newly generated `Jwt__Key` of at least 32 characters. The issuer and audience may remain `CampusFix`.
4. Supply `Seed__StaffEmail` and `Seed__StaffPassword` only if creating the first staff account. Store these as hosting secrets. Remove the seed password after successful initialization. A later seed change does not reset an existing account.
5. Configure the web environment with `ASPNETCORE_ENVIRONMENT=Production`, `ASPNETCORE_HTTP_PORTS=8080` and `Api__BaseUrl` pointing to the reachable API URL, including the trailing slash. Use the provider's HTTPS address. Keep `Cookies__RequireSecure=true` for the live site; the Compose `false` override is only for local HTTP. On Render's controlled HTTPS proxy, set `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` so ASP.NET Core recognises the original HTTPS request. Only use this shortcut when all incoming requests pass through the trusted platform edge, which must overwrite incoming forwarded headers. For other environments, explicitly configure trusted proxy addresses/networks instead of trusting arbitrary senders; see [Microsoft's proxy configuration guidance](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0).
6. Use `/health` as each service's health path. Configure hostnames/HTTPS and verify the provider routes requests to port 8080. Keep the two services compatible when releasing changes.
7. Disable the provider's independent automatic deploy trigger so the GitHub workflow controls updates after checks pass. Complete any initial deployment and verify it before relying on the pipeline.

The web app writes session encryption keys under `App_Data`. Without persistent shared key storage, restarting/replacing a container can sign users out. Configure durable protected key storage before scaling to multiple web instances. This student scaffold does not configure backups, password recovery, production monitoring or database schema migrations.

## Enable deployment after CI

Retrieve each service's secret deploy hook from the provider dashboard and add these **GitHub Actions repository secrets**:

| Secret | Value |
|---|---|
| `RENDER_API_DEPLOY_HOOK` | API service's complete deploy-hook URL |
| `RENDER_WEB_DEPLOY_HOOK` | Web service's complete deploy-hook URL |

Do not paste either URL into source files, screenshots or the README. They authorize deployment. The workflow requests the exact tested commit using the `ref` parameter supported by [Render deploy hooks](https://render.com/docs/deploy-hooks).

The deployment job runs on a push to `main` after build/tests, source security and container checks all succeed. If either secret is missing, its job summary explicitly says **Not deployed**. With both secrets present, accepted hooks produce **Deployment requested**; check the hosting dashboard for completion. The workflow does not poll deployment status or certify the live application.

## Record real evidence

- Confirm both live health endpoints succeed.
- Register, report a fault, update it as staff and verify student isolation on the live site.
- Restart/redeploy and confirm database records persist.
- Verify a real PR merge triggers passing checks and a completed live deployment.
- Add the public web URL and actual deployment date to the README; never include credentials.

Keep the database and application versions consistent. The initial schema uses `EnsureCreated`; introduce and test migrations before changing a database that contains records you need to preserve.
