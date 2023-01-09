# Example: Simple Server

This is a simple example. The server receives data, counts the number of words and responds to the client. Not exactly a real-life scenario but it demonstrates the usage of adapters/handlers.

Data received goes through `WordAdapter` that translates a byte array into a string and forwards a collection of words.

Here's the workflow

- The `WordAdapter` receives a byte array and translates it into a string
- The adapter counts the number of words and forwards a `MatchCollection` containing the words
- The `WordHandler` receives a `MatchCollection` and computes an adequate response
- The handler writes the response to the output pipeline as a string
- The `UTFEncoderAdapter` receives the string and forwards it as an UTF8 byte array
- The byte array is sent back to the channel

Telnet can be used to test the communication with the server.

```bash
$ telnet localhost 8080  
The quick brown fox jumps over the lazy dog
received 9 word(s).
```
