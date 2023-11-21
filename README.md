# HootNCommute: Corporate Carpooling

This readme will describe the details of the CorPool system: the architecture and all components, the implementation details, the infrastructure and its details, and, of course, how to run it yourself.

## Architecture

The CorPool system has 6 main components: a custom developed backend, frontend, and worker service; in addition it uses RabbitMQ as message queue, Redis as cache, and MongoDB as primary storage.

### Front-end

The front-end is an SPA built in Vue.js, nothing too special. It communicates to the backend mostly via a REST api, but for some parts also using Microsofts SignalR, which is an RMI/RPC library that makes use of WebSockets, or Server-Sent Events or Long Polling as fallback mechanisms. In this application, only the WebSockets are used to facilitate fully stateless load balancers and backend implementations.

### Back-end

The back-end is an ASP.NET Core 7 application written in C#. The back-end interconnects all components in the system, but mostly only performs simple CRUD operations. It is designed to be fully stateless, so it can fail easily and quickly when something goes wrong.

### Worker

The workers are a component that perform more advanced tasks, in this case it will perform the route finding procedure. In the current form, this algorithm is quite simple, but this could become quite an extensive and complicated procedure that demands more computing power than acceptable within the timeframe of a simple API request. The workers retrieve ride requests from RabbitMQ, find an appropriate ride offer from MongoDB, and send the result back to the user via Redis.

## Technologies

### SignalR

As mentioned before, SignalR is a RMI library by Microsoft that makes use of WebSockets internally. This simplifies the use and deployment of WebSockets in an application, and it integrates very nicely with the rest of the .NET Core framework via DI and other services.

SignalR is designed to serve not only one-on-one connections between server and client, but it also provides the option to communicate between connections. On a redundant system, this communication goes via a so-called backplane that allows each component to be aware of all available connections. This enables us to send an offer found in the Worker to the front-end via the back-end without any additional coding in the back-end.

### ASP.NET Core Identity

The use of the Identity system in .NET Core allows us to easily provide authentication and authorization functionality in CorPool. We use MongoDB as the backing store, and it will natively offer the use of JWT for authentication instead of session cookies, allowing for a fully stateless backend. The JWTs (and their corresponding expiration date) are stored in the localstorage on the user device.

### Multitenancy

The multitenancy in CorPool is achieved within the framework itself by implementing a custom middleware for .NET Core. The implementation of this can be found in the folder `AspNetCoreTenant`. This system allows all CRUD operations to be tenant-aware without much explicit coding, limiting the chance that a programmer error leaks data between tenants.

The middleware detects the current tenant based on the subdomain of the url, in the form of `<tenant>.<domainname>/<query>`, and injects it into the current `HttpContext`. This allows other components to easily obtain the current tenant, for example the `DatabaseContext` offers a `Tenanted` function that will automatically filter all MongoDB queries to the current tenant.

The main place where tenant-aware code is required, is upon insertion and update of new data in the database. This could be improved when using an SQL database, which would allow for more automation in the ORM layer using Entity Framework.

## Run it yourself

For this barebones version, only the databases are running in Docker to save you the trouble of installing three database systems on your machine. The app, as is, will probably run on both Windows and Linux, but it is recommend to run in Linux or WSL. To get the app up-and-running, you will need to have installed the following tools:

- Git
- [.NET 7 SDK + Runtime and ASP.NET Core SDK + Runtime](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-2204).
- Docker and Docker Compose (Compose is included in "Docker Desktop"; when running in WSL please install the Windows version of Docker and enable its WSL integration)
- [Node Version Manager](https://github.com/nvm-sh/nvm)
- In addition, it is recommended to install an editor or IDE, such as VS Code, Rider, or Visual Studio itself when you are running Windows.

So first, clone the repository from GitLab. Ideally, you should upload your SSH key to your GitLab profile, to prevent working with temporary access keys and such.

### Databases

To get started, first start the databases using `docker compose up -d` (the `-d` flag makes it run in the background). This should download the database files and start 4 containers: the three databases and a reverse proxy. The reverse proxy is used to prevent CORS issues between the frontend and backend - it will automatically redirect requests to either the backend or the frontend depending on the URL.

### Backend

Then, you should start the backend. For this, `cd backend` and run `dotnet run`. This should start the backend server at port 33081.

### Frontend

Open a new terminal window to start the frontend. `cd frontend`, then run `nvm install` to install the required version of Node.JS for this project. NVM will automatically manage these versions for you, so it doesn't conflict with any pre-existing tools on your machine. Any future time you access this folder, you'll want to run `nvm use` to activate the installation.

Install Yarn using `npm install --global yarn`. You can now install the packages for this project using `yarn install`.

Start the frontend using `yarn serve`, after which it will be available at port 8080.

### Workers

Lastly, you'll want to spin up at least one worker instance for resolving ride requests. To do so, in a new terminal window, `cd Worker` and run `dotnet run`.

### Access

Once everything is up and running, you should open a browser and access `localhost:33080`. This is the address at which the reverse proxy (hosted in Docker) is listening. This proxy, as mentioned before, will make sure to redirect requests to either the frontend or backend, depending on the URL.

Once you see the interface, click the "Seed Database" button. This will fill the database with some default users and tenants, as specified in `backend/Controllers/SeedController.cs`. After this point, you should always access the interface through a _tenant-specific domain_. Simply add the tenant name as a subdomain to your browser url, e.g. `rug.localhost:33080`.

Once you access the website through a specific tenant, you can log in. The UI sadly does not distinguish between being logged in or not, but if you try to open the "Profile" tab without being logged in you will be greeted with a 401 error. Click the "logout" button to login (yes, really) - use one of the users specified in the seed, together with the password `password`. Feel free to improve this experience.

As a TL;DR for the seed: it creates tenants "ah" and "rug", with uses "arnold", "vasilios", "alexander", "jorge", "student" for "rug", and "student2", "friend", "boss" for "ah". The password is always just "password".
