# Example: Web Sockets Server

This is a simple example. The server receives data, counts the distinct letters and sends the result back.

Two websocket endpoints are exposed:

- `/ws/uppercase` receives a string and returns the count of distinct letters in uppercase
- `/ws/lowercase` receives a string and returns the count of distinct letters in lowercase

Here's the workflow

- The `LetterAdapter` receives a websocket message, decodes its content into a string and forwards a char array
- The `LowercaseAdapter` receives the char array and converts it to lowercase
- The `LetterHandler` receives the char array and computes an adequate response as a string
- Internal middleware transforms the string into a websocket message and sends it back to the client

Postman can be used to test the communication with the server.
