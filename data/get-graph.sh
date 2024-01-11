wget  -O graph.csdl.xml 'https://graph.microsoft.com/v1.0/$metadata?$format=xml'  

curl -o graph.csdl.xml 'https://graph.microsoft.com/v1.0/$metadata?$format=xml'


iwr 'https://graph.microsoft.com/v1.0/$metadata?$format=xml' -OutFile graph.csdl.xml