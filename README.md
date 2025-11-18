Run ATM Test Project

1. Start API
The API was built using
- .net 9 sdk

From inside main repo directory

```
cd ATMAPI
dotnet run --urls "http://localhost:5056"
```

this shoud start the API on http://localhost:5056

2. Start client
The client app was built using
- Node v24.11.1
- nvm v1.2.2
- npm v11.6.2 (installed as part of node)

```
cd ATMClient
npm install
npm run dev -- --port=5173
```

this should start the client on http://localhost:5173/

Note:
The app uses SQL lite and data is wiped on every restart of the API service
