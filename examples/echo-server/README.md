# Example: Echo Server

This is a simple example where the server simply echoes back the received data. Since we're not doing anything with the data, there are no adapters, just a handler that writes the received data to the output buffer.

Telnet can be used to test the communication with the server. Everything sent should be replied back.

```bash
$ telnet localhost 8080  
hello
hello
My name is John, John Doe
My name is John, John Doe
```
