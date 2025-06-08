# FENIX-MCDU

This uses Fenix's browser-based version of the EFB to show the display for, and
send input to, either the pilot or co-pilot MCDU.

It always starts off showing whichever MCDU the device defaults to (if it's
configured then it shows the First Officer's MCDU). You can toggle between pilot
and co-pilot with the top-right BLANK button.

The sample uses the `GraphQL.Client` library to manage communication with Fenix's
EFB:

https://github.com/graphql-dotnet/graphql-client

I'm not aware of Fenix documenting the EFB or any of the GraphQL stuff, so this
could easily break with any Fenix update, even a minor one.
