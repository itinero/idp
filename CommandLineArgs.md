 Itinero Data Processor 
 ====================== 

The **Itinero Data Processor** *(IDP)* helps to convert a routable graph into a RouterDB, which can be used to quickly solve routing queries.

To work with IDP, you only need an input graph. OpenStreetMap data dumps can be obtained at [geofrabrik.de](http://download.geofabrik.de/)


Typical usage:

        IDP --read-pbf <input-file> --pr --create-routerdb bicycle --write-routerdb output.routerdb

To include elevation data, add `--elevation`. To solve the queries even faster, use `--contract bicycle.<profile-to-optimize>`.  


Switch Syntax
-------------

The syntax of a switch is:

    --switch param1=value1 param2=value2
    # Or equivalent:
    --switch value1 value2


There is no need to explicitly give the parameter name, as long as the *unnamed* parameters      are in the same order as in the tables below. Note that you are free to name some (but not all) arguments. `--switch value2 param1=value1`, `--switch value1 param2=value2` or `--switch param1=value1 value2` are valid just as well.

At last, `-param1` is a shorthand for `param=true`. This is useful for flags



 Full overview of all options 
 ------------------------------- 

All switches are listed below. Click on a switch to get a full overview, including sub-arguments.

- [Input](#Input)
  * [--read-pbf](#--read-pbf---rb) Reads an OpenStreetMap input file.
  * [--read-shape](#--read-shape---rs) Read a shapefile as input to do all the data processing.
  * [--read-routerdb](#--read-routerdb) Reads a routerdb file for processing.
- [Data processing](#Data-processing)
  * [--create-routerdb](#--create-routerdb) Converts an input source (such as an `osm`-file) into a routable graph.
  * [--elevation](#--elevation---ele) Incorporates elevation data in the calculations.
  * [--contract](#--contract) Applies contraction on the graph.
- [Data analysis](#Data-analysis)
  * [--islands](#--islands-Experimental-feature) Detects islands in a routerdb.
- [Output](#Output)
  * [--write-routerdb](#--write-routerdb) Specifies that the routable graph should be saved to a file.
  * [--write-pbf](#--write-pbf---wb) Writes the result of the calculations as protobuff-osm file.
  * [--write-shape](#--write-shape) Write the result as shapefile
  * [--write-geojson](#--write-geojson---wg) Write a file as geojson file.
- [Usability](#Usability)
  * [--progress-report](#--progress-report---progress---pr) If this flag is specified, the progress will be printed to standard out.
  * [--log](#--log) If specified, creates a logfile where all the output will be written to - useful to debug a custom routing profile
  * [--help](#--help---?) Print the help message
### Input
#### --read-pbf (--rb)

   Reads an OpenStreetMap input file. The format should be an `.osm.pbf` file.

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| **file** | _Obligated param_ | The .osm.pbf file that serves as input | 

#### --read-shape (--rs)

   Read a shapefile as input to do all the data processing.

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| **file** | _Obligated param_ | The input file to read | 
| **vehicle** | _Obligated param_ | The profile to read. This can be a comma-separated list too. | 
| **svc** | _Obligated param_ | The `source-vertex-column` | 
| **tvc** | _Obligated param_ | The `target-vertex-column` | 

#### --read-routerdb

   Reads a routerdb file for processing. This can be useful to e.g. translate it to a geojson or shapefile.

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| **file** | _Obligated param_ | The path where the routerdb should be read. | 
| mapped, m | `false`| Enable memory-mapping: only fetch the parts from disk that are needed. There is less memory used, but the queries are slower. | 

### Data processing
#### --create-routerdb

   Converts an input source (such as an `osm`-file) into a routable graph. If no vehicle is specified, `car` is used.
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

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| vehicle, vehicles | `car`| The vehicle (or comma separated list of vehicles) that the routing graph should be built for. | 
| keepwayids, wayids | `false`| Boolean indicating that the way IDs should be kept | 
| allcore | `false`| Boolean indicating allcore | 
| simplification | `1`| Integer indicating the simplification factor. Default: very small | 
| normalize | `false`| Normalize the values. | 

#### --elevation (--ele)

   Incorporates elevation data in the calculations.
Specifying this flag will download the SRTM-dataset and cache this on the file system.This data will be reused upon further runs

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| cache | `srtm-cache`| Caching directory name, if another caching directory should be used. | 

#### --contract

   Applies contraction on the graph.Solving queries on a contracted graph is _much_ faster, although preprocessing is quite a bit slower (at least 5 times slower);most use cases will require this flag.To enable contraction for multiple profiles and/or multiple vehicles, simply add another --contraction

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| **profile** | _Obligated param_ | The profile for which a contraction hierarchy should be built | 
| augmented | `false`| If specified with 'yes', an augmented weight handler will be used | 

### Data analysis
#### --islands (Experimental feature)

   Detects islands in a routerdb. An island is a subgraph which is not reachable via the rest of the graph.

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| profile | _NA_| The profile for which islands should be detected. This can be a comma-separated list of profiles as well. Default: apply island detection on _all_ profiles in the routerdb | 

### Output
#### --write-routerdb

   Specifies that the routable graph should be saved to a file. This routerdb can be used later to perform queries.

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| **file** | _Obligated param_ | The path where the routerdb should be written. | 

#### --write-pbf (--wb)

   Writes the result of the calculations as protobuff-osm file. The file format is `.osm.pbf`

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| **file** | _Obligated param_ | The file to write the .osm.pbf to | 

#### --write-shape

   Write the result as shapefile

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| **file** | _Obligated param_ | The output file to write to | 

#### --write-geojson (--wg)

   Write a file as geojson file. Useful for debugging

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| **file** | _Obligated param_ | The output file which will contain the geojson. If the file already exists, it will be overwritten without warning. | 
| left | _NA_| Specifies the minimal latitude of the output. Used when specifying a bounding box for the output. | 
| right | _NA_| Specifies the maximal latitude of the output. Used when specifying a bounding box for the output. | 
| top, up | _NA_| Specifies the minimal longitude of the output. Used when specifying a bounding box for the output. | 
| bottom, down | _NA_| Specifies the maximal longitude of the output. Used when specifying a bounding box for the output. | 

### Usability
#### --progress-report (--progress, --pr)

   If this flag is specified, the progress will be printed to standard out. Useful to see how quickly the process goes and to do a bit of initial troubleshooting.



*This switch does not need parameters*

#### --log

   If specified, creates a logfile where all the output will be written to - useful to debug a custom routing profile

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| file | `log.txt`| The name of the file where the logs will be written to | 

#### --help (--?)

   Print the help message

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| about | _NA_| The command (or switch) you'd like more info about | 
| markdown, md | _NA_| Write the help text as markdown to a file. The documentation is generated with this flag. | 

