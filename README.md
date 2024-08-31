# CLI tool to generate formatted yaml files from a given configuration file

This is a DevOps tool that generates formatted yaml or json files from a given configuration file.
This tools is useful then you need to expose the the user a config file and need to transform that normal config file into another format or perform some operations on it.

## Installation

```bash
dotnet tool install -g ConfigSetter
```

## Usage

Create a config file that dictates the structure of the output file and the expected input. The config file is a yaml file that contains the following fields:
```yaml
setup:
  json_patch:
    - { "op": "add", "path": "/container", "value": {} }
    - { "op": "add", "path": "/container/resources", "value": {} }
    - { "op": "add", "path": "/container/resources/requests", "value": {} }
    - { "op": "add", "path": "/container/resources/limits", "value": {} }

settings:
  - name: MEMORY_LIMIT
    type: memory
    json_patch:
      - { "op": "add", "path": "/container/resources/limits/memory" }
  - name: CPU_LIMIT
    type: cpu
    json_patch:
      - { "op": "add", "path": "/container/resources/limits/cpu" }
  - name: MEMORY_REQUESTS
    type: memory
    json_patch:
      - { "op": "add", "path": "/container/resources/requests/memory" }
  - name: CPU_REQUESTS
    type: cpu
    json_patch:
      - { "op": "add", "path": "/container/resources/requests/cpu" }
```

Have the tool parse a settings file and generate the output file:

```yaml
# Settings yaml file:
variables:
  DEV_CPU_LIMIT: "15m"
  DEV_MEMORY_LIMIT: "128M"
  PROD_CPU_LIMIT: "50m"
  PROD_MEMORY_LIMIT: "512M"
```

Run the tool:
```bash
config-setter -v -c ./config.yaml -i ./settings.yaml --silent --prefix DEV
```

The output file will be:
```yaml
container:
  resources:
    requests: {}
    limits:
      cpu: 15m
      memory: 128M
```