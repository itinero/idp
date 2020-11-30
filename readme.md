Itinero Data Processor
======================

IDP is a simple tool that exposes some of Itinero's functionality to act as a CLI tool to process routing data.

Basic supported features are:

- Reading OSM-XML (.osm).
- Reading OSM-PBF (.osm.pbf).
- Creating routing databases for Itinero.

Everything IDP can, you can also do by writing code and using OsmSharp and Itinero but this tool allows you to setup regular update processes for example.

### Usage

You can download the latest builds here:

http://files.itinero.tech/builds/idp/

##### A few examples

Build a RouterDb for cars:

`idp --read-pbf path/to/some-file.osm.pbf --pr --create-routerdb vehicles=car --write-routerdb some-file.routerdb`

- read-pbf: reads an OSM-PBF file.
- pr: shows progress information.
- create-routerdb: creates a routerdb in this case only for cars.
- write-router: writes the routerdb to disk.

Build a RouterDb for pedestrians and bicycle and add a contracted graph for both:

`idp --read-pbf path/to/some-file.osm.pbf --pr --create-routerdb vehicles=bicycle,pedestrian --contract bicycle --contract pedestrian --write-routerdb some-file.routerdb`

Adding elevation from SRTM can be done using:

`idp --read-pbf path/to/some-file.osm.pbf --pr --create-routerdb vehicles=bicycle --elevation --write-routerdb some-file.routerdb`

- contract: adds a contracted version of the routing graph to the routerdb.

### Related projects

- [routing](https://github.com/itinero/routing): The core routing project, used by all other projects.
- [idp](https://github.com/itinero/idp): The data processing project, a CLI tool to process data into routerdb's.
- [routing-api](https://github.com/itinero/routing-api): A routing server that can load routerdb's and accept routing requests over HTTP.
