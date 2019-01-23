 Itinero Data Processor 
 ====================== 

The **Itinero Data Processor** *(IDP)* helps you to convert a routable graph into a RouterDB, which can help to quickly solve routing queries.

To work with IDP, you need:

- An input graph. OpenStreetMap data dumps can be obtained at [geofrabrik.de](http://download.geofabrik.de/)
- A routing profile, which can be obtained in [our repo](https://github.com/anyways-open/routing-profiles/) 

Typical usage:

        IDP --read-pbf <input-file> --pr --create-routerdb bicycle.lua --write-routerdb output.routerdb

Often in combination with `--contract bicycle.networks` and `--elevation` for production.


 Full overview of all options 
 ------------------------------- 

### --read-pbf (--rb)
   Read an .osm.pbf file to serve as input

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| **file** | ✓ | The .osm.pbf file that serves as input | 

### --contract
   Applies contraction on the graph.Solving queries on a contracted graph is _much_ faster, although preprocessing is quite a bit slower (at least 5 times slower);most use cases will require this flag.To enable contraction for multiple profiles and/or multiple vehicles, simply add another --contraction

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| **profile** | ✓ | The profile for which a contraction hierarchy should be built | 
| augmented | | If specified with 'yes', an augmented weight handler will be used | 

### --write-pbf (--wb)
   Write an .osm.pbf file

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| **file** | ✓ | The file to write the .osm.pbf to | 

### --write-geojson (--wg)
   Write a file as geojson file. Useful for debugging

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| **file** | ✓ | The output file which will contain the geojson. Will be overriden by the code | 
| left | | Specifies the minimal latitude of the output. Used when specifying a bounding box for the output. | 
| right | | Specifies the maximal latitude of the output. Used when specifying a bounding box for the output. | 
| top | | Specifies the minimal longitude of the output. Used when specifying a bounding box for the output. | 
| bottom | | Specifies the maximal longitude of the output. Used when specifying a bounding box for the output. | 

### --progress-report (--progress, --pr)
   If this flag is specified, the progress will be printed to standard out. Useful to see how quickly the process goes and to do a bit of initial troubleshooting.



*This switch does not need parameters*

### --log
   If specified, creates a logfile where all the output will be written to - useful to debug a custom routing profile

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| **file** | ✓ | The name of the file where the logs will be written to | 

### --help (--?)
   Print the help message

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| about | | The command (or switch) you'd like more info about | 
| markdown | | Output the help text in markdown | 

