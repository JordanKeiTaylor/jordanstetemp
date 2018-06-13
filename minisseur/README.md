# minisseur
Hacky little tool to run Regisseur expectations against a prometheus instance for use with `spatial local cluster`.

## To run

```
TERM=dumb ./gradlew -q run -PappArgs="['<config.pb.json>']"
```

## To build a distribution

```
./gradlew distZip
```

