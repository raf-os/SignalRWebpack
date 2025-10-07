Simple backend in c# using the asp.net framework, using SignalR for a real-time app. Also has an api endpoint for the client's dashboard fetch requests.

Contains some basic, incredibly insecure auth system which is acceptable for a test app. Basically, it creates a login token when an user logs in, and that's what the client uses for authorization checks. By making a call to the server with that token, that is, it's not client-sided.

Uses an sqlite database to store user data.

Client code can be viewed [here](https://github.com/raf-os/React-SignalR-ChatApp).