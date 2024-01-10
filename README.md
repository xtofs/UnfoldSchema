

# how to try

* cd to `Unfold` firectory
* run `dotnet run -- ../data/example89.csdl.xml `

There are more example input files in /data.
The full graph metadata can be downloaded from `https://graph.microsoft.com/v1.0/$metadata`

the output is saved in `output` (in the direcotry that the `dotnet run`` is executed) with a file name that starts with the input file name 


# Schema graph text file
The program creates and in memory graph that represents how to navigate from one type to another. i.e. which URL path sagement can be legally appended to a URL that retrieves an instance of the type returned by the URL.

the file lists:
- each nodes in the form of `[<fully qualified type name>]`.
- each navigation from this type as an idented list 
  - of the form `<segment name> -> [<target fully qualified type name>]`

e.g. 
```
[ODataDemo.Address]
	country -> [ODataDemo.Country] 
```
indicates that 
- there is a type `ODataDemo.Address`
- if a URL return that type, it can be extended 
  - with a segment named `country`
  - which returns an instance of type `ODataDemo.Country`



# REFERENCES

https://docs.oasis-open.org/odata/odata-csdl-xml/v4.01/odata-csdl-xml-v4.01.html#sec_CSDLExamples
