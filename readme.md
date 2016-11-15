Itinero Data Processor
======================

IDP is a simple tool that exposes some of Itinero's functionality to act as a CLI tool to process routing data.

Basic supported features are:

- Reading OSM-XML (.osm).
- Reading OSM-PBF (.osm.pbf).
- Creating routing databases for Itinero.

Everything IDP can, you can also do by writing code and using OsmSharp and Itinero but this tool allows you to setup regular update processes for example.

### Related projects

- [routing](https://github.com/itinero/routing): The core routing project, used by all other projects.
- [idp](https://github.com/itinero/idp): The data processing project, a CLI tool to process data into routerdb's.
- [routing-api](https://github.com/itinero/routing-api): A routing server that can load routerdb's and accept routing requests over HTTP.
