 Itinero Data Processor 
 ====================== 

The **Itinero Data Processor** *(IDP)* helps you to convert a routable graph into a RouterDB, which can help to quickly solve routing queries.

To work with IDP, you only need an input graph. OpenStreetMap data dumps can be obtained at [geofrabrik.de](http://download.geofabrik.de/)


Typical usage:

        IDP --read-pbf <input-file> --pr --create-routerdb bicycle --write-routerdb output.routerdb

Often in combination with `--contract bicycle.networks` and `--elevation` for production.


 Full overview of all options 
 ------------------------------- 

- [Input](#Input)
- [Data processing](#Data processing)
- [Output](#Output)
- [Usability](#Usability)
### Input
#### --read-pbf (--rb)
   Read an .osm.pbf file to serve as input

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| **file** | ✓ | The .osm.pbf file that serves as input | 

#### --read-routerdb
   Reads a routerdb file for processing, e.g. to translate it to a geojson.

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| **file** | ✓ | The path where the routerdb should be read. | 
| mapped | | Enable memory-mapping: only fetch the parts from disk that are needed. There is less memory used, but the queries are slower. Use 'mapped=true' | 
| m | | Same as 'mapped'. | 

### Data processing
#### --create-routerdb
   Converts an input source (such as a .osm) into a routable graph. If no vehicle is specified, `car` is used.
If the routing graph should be built for another vehicle, the `vehicle`-parameter can be used

1) specify a **file** containing a routing profile [(examples in our repository)](https://github.com/anyways-open/routing-profiles/), or...
2) a **built-in** profile can be used. This should be one of:

 - `Bicycle`
 - `BigTruck`
 - `Bus`
 - `Car`
 - `Moped`
 - `MotorCycle`
 - `Pedestrian`
 - `SmallTruck`

Additionally, there are two special values:

- `all`: Adds all of the above vehicles to the routing graph
- `motors `(or `motorvehicles`): adds all motor vehicles to the routing graph

Note that one can specify multiple vehicles at once too, using the `vehicles` parameter (note the plural)

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| vehicle | | The vehicle that the routing graph should be built for. Default is 'car'. | 
| vehicles | | A comma separated list containing vehicles that should be used | 
| keepwayids | | Boolean indicating that the way IDs should be kept | 
| wayids | | Same as `keepwayids` | 
| allcore | | Boolean indicating allcore | 
| simplification | | Integer indicating the simplification factor. Default: very small | 

#### --elevation (--ele)
   Incorporates elevation data in the calculations.
Specifying this flag will download the SRTM-dataset and cache this in srtm-cache.This data will be reused upon further runs

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| cache | | Caching directory name, if another caching directory should be used. | 

#### --contract
   Applies contraction on the graph.Solving queries on a contracted graph is _much_ faster, although preprocessing is quite a bit slower (at least 5 times slower);most use cases will require this flag.To enable contraction for multiple profiles and/or multiple vehicles, simply add another --contraction

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| **profile** | ✓ | The profile for which a contraction hierarchy should be built | 
| augmented | | If specified with 'yes', an augmented weight handler will be used | 

### Output
#### --write-routerdb
   Specifies that the routable graph should be saved to a file. This routerdb can be used later to perform queries.

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| **file** | ✓ | The path where the routerdb should be written. | 

#### --write-pbf (--wb)
   Write an .osm.pbf file

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| **file** | ✓ | The file to write the .osm.pbf to | 

#### --write-geojson (--wg)
   Write a file as geojson file. Useful for debugging

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| **file** | ✓ | The output file which will contain the geojson. Will be overriden by the code | 
| left | | Specifies the minimal latitude of the output. Used when specifying a bounding box for the output. | 
| right | | Specifies the maximal latitude of the output. Used when specifying a bounding box for the output. | 
| top | | Specifies the minimal longitude of the output. Used when specifying a bounding box for the output. | 
| bottom | | Specifies the maximal longitude of the output. Used when specifying a bounding box for the output. | 

### Usability
#### --progress-report (--progress, --pr)
   If this flag is specified, the progress will be printed to standard out. Useful to see how quickly the process goes and to do a bit of initial troubleshooting.



*This switch does not need parameters*

#### --log
   If specified, creates a logfile where all the output will be written to - useful to debug a custom routing profile

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| **file** | ✓ | The name of the file where the logs will be written to | 

#### --help (--?)
   Print the help message

| Parameter  | Obligated? | Explanation       |
|----------- | ---------- | ----------------- |
| about | | The command (or switch) you'd like more info about | 
| markdown | | Write the help text as markdown to a file | 

