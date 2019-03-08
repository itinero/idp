 Itinero Data Processor 
 ====================== 

The **Itinero Data Processor** *(IDP)* helps to convert a routable graph into a RouterDB, which can be used to quickly solve routing queries.

The minimal requirement to work with IDP is having a routable graph to serve as input. OpenStreetMap data for the entire world can be obtained for free at [geofrabrik.de](http://download.geofabrik.de/)


 Some examples
 -------------
A minimal example which builds routing for bicycles is

        IDP --read-pbf <input-file.osm.pbf> --pr --create-routerdb bicycle --write-routerdb output.routerdb

To include elevation data, add `--elevation`. To solve the queries even faster, use `--contract bicycle.<profile-to-optimize>`.
The full command would thus become

        IDP --read-pbf <input-file.osm.pbf> --pr --elevation --create-routerdb bicycle --contract bicycle.fastest --write-routerdb output.routerdb

For more advanced options, see the arguments below.

Switch Syntax
-------------

The syntax of a switch is:

    --switch param1=value1 param2=value2
    # Or equivalent:
    --switch value1 value2


There is no need to explicitly give the parameter name, as long as *unnamed* parameters are in the same order as in the tables below. It doesn't mater if only some arguments, all arguments or even no arguments are named. `--switch value2 param1=value1`, `--switch value1 param2=value2` or `--switch param1=value1 value2` are valid just as well.

At last, `-param1` is a shorthand for `param=true`. This is useful for boolean flags



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
  * [--islands](#--islands) Detects islands in a routerdb.
- [Output](#Output)
  * [--write-routerdb](#--write-routerdb) Specifies that the routable graph should be saved to a file.
  * [--write-pbf](#--write-pbf---wb) Writes the result of the calculations as protobuff-osm file.
  * [--write-shape](#--write-shape) Write the result as shapefile
  * [--write-geojson](#--write-geojson---wg) Write a file as geojson file.
- [Transit-Db](#Transit-Db)
  * [--create-transit-db](#--create-transit-db---create-transit---ct) Creates or updates a transit DB based on linked connections.
  * [--read-transit-db](#--read-transit-db--read-transit---rt) Read a transitDB file as input to do all the data processing.
  * [--select-time](#--select-time) Filters the transit-db so that only connections departing in the specified time window are kept.
  * [--dump-locations](#--dump-locations) Writes all stops contained in a transitDB to console
  * [--dump-connections](#--dump-connections) Writes all connections contained in a transitDB to console
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

   Read a shapefile as input to do all the data processing.To tie together all the edges, the endpoint of each edge should have an identifier. If two edges share an endpoint (and thus allow traffic to go from one edge to the other), the identifier for the common endpoint should be the same. The attributes which identify the start- and endpoint should be passed explicitly in this switch with `svc` and `tvc`

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| **file** | _Obligated param_ | The input file to read | 
| **vehicle** | _Obligated param_ | The profile to read. This can be a comma-separated list too. | 
| **svc** | _Obligated param_ | The `source-vertex-column` - the attribute of an edge which identifies one end of the edge. | 
| **tvc** | _Obligated param_ | The `target-vertex-column` - the attribute of an edge which identifies the other end of the edge. | 

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
| keepwayids, wayids | `false`| If specified, the identifiers in the source data are kept. By default, they are discarded. Specify this flag if the calculated routes will have to be backreferenced to the source data set. | 
| allcore | `false`| If true, all nodes in the source data will be converted into vertices, even if they have only two neighbours.By default, only nodes at intersections will be kept in the routerdb as vertices. Nodes with only two neighbours are just part of a road and skipped. | 
| simplification | `1`| Parameter to steer simplification. Simplification removes points from each edge in order to have simpler (yet similar) lines using [Ramer-Doublas-Peucker](https://en.wikipedia.org/wiki/Ramer%E2%80%93Douglas%E2%80%93Peucker_algorithm) | 
| normalize | `false`| When building the routerdb, a table is built for each combination of tags. E.g. `highway=residential` will get entry `1`, whereas `highway=residential & access=public` will get entry `2`. If this table has to be kept small, `normalize` can be used. Tags will be rewritten to equivalent forms. In our example, `access=public` will be dropped, as this is implied by `highway=residential`. | 

#### --elevation (--ele)

   Incorporates elevation data in the calculations.
Specifying this flag will download the SRTM-dataset and cache this on the file system.This data will be reused upon further runs

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| cache | `srtm-cache`| Caching directory name, if another caching directory should be used. | 

#### --contract

   Applies contraction on the graph.Solving queries on a contracted graph is _much_ faster, although preprocessing is quite a bit slower (at least 5 times slower);most use cases will require this flag.To enable contraction for multiple profiles and/or multiple vehicles, simply add another --contraction

Contraction is able to speed up querying by building an index of _shortcuts_. Basically, between some points of the graph, an extra vertex is inserted in the routerdb.This extra vertex represents how one could travel between these points and which path one would thus take.The actual search for a shortest path can use these shortcuts instead of searching the whole graph. For more information, see [the wikipedia article on contraction hierarchies](https://en.wikipedia.org/wiki/Contraction_hierarchies)

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| **profile** | _Obligated param_ | The profile for which a contraction hierarchy should be built | 
| augmented | `false`| By default, only one metric is kept in the hierarchy - such as either time or distance (which one depends on the profile). For some usecases, it is useful to have _both_ distance and time available in the routerdb. Setting this flag to `true` will cause both metrics to be included. | 

### Data analysis

#### --islands

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

### Transit-Db

#### --create-transit-db (--create-transit, --ct)

   Creates or updates a transit DB based on linked connections. For this, the linked connections source and a timewindow should be specified.
If the previous switch reads or creates a transit db as well, the two transitDbs are merged into a single one.

Note that this switch only downloads the connections and keeps them in memory. To write them to disk, add --write-transit-db too.

Example usage to create the database for the Belgian SNCB:

        idp --create-transit-db https://graph.irail.be/sncb/connections https://irail.be/stations/NMBS

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| **connections, curl** | _Obligated param_ | The URL where connections can be downloaded | 
| **locations, lurl** | _Obligated param_ | The URL where the location can be downloaded | 
| window-start, start | `now`| The start of the timewindow to load. Specify 'now' to take the current date and time. | 
| window-duration, duration | `3600`| The length of the window to load, in seconds. If zero is specified, no connections will be downloaded. | 

#### --read-transit-db (-read-transit, --rt)

   Read a transitDB file as input to do all the data processing. A transitDB is a database containing connections between multiple stops

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| **file** | _Obligated param_ | The input file to read | 

#### --select-time

   Filters the transit-db so that only connections departing in the specified time window are kept. This allows to take a small slice out of the transitDB, which can be useful to debug. All locations will be kept.

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| **window-start, start** | _Obligated param_ | The start time of the window | 
| **duration** | _Obligated param_ | The length of the time window. | 
| allow-empty | `false`| If flagged, the program will not crash if no connections are retained | 

#### --dump-locations

   Writes all stops contained in a transitDB to console

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| file | _NA_| The file to write the data to, in .csv format | 

#### --dump-connections

   Writes all connections contained in a transitDB to console

| Parameter  | Default value | Explanation       |
|----------- | ------------- | ----------------- |
| file | _NA_| The file to write the data to, in .csv format | 

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
| experimental | `false`| Include experimental switches in the output | 

