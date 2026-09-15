# Krosoft.ObjectStorage.CLI

[![forthebadge](https://forthebadge.com/badges/built-with-love.svg)](https://forthebadge.com) [![forthebadge](https://forthebadge.com/badges/made-with-c-sharp.svg)](https://forthebadge.com)

Outil CLI pour interagir avec un stockage objet compatible S3 (MinIO, AWS S3, etc.).

## Profil de connexion

Toutes les commandes nécessitent un fichier de profil JSON via l'option `--profile`.

```json
{
  "name": "local",
  "objectStorage": {
    "endpoint": "http://localhost:9000",
    "accessKey": "YOUR_ACCESS_KEY",
    "secretKey": "YOUR_SECRET_KEY",
    "useSSL": false,
    "timeout": 10
  }
}
```

| Champ       | Type     | Requis | Description                         |
| ----------- | -------- | ------ | ----------------------------------- |
| `endpoint`  | `string` | oui    | URL de l'instance S3/MinIO          |
| `accessKey` | `string` | oui    | Clé d'accès                         |
| `secretKey` | `string` | oui    | Clé secrète                         |
| `useSSL`    | `bool`   | non    | Active HTTPS (défaut : `false`)     |
| `timeout`   | `int`    | non    | Timeout en secondes (défaut : `10`) |

---

## Commandes

### `info`

Affiche les informations générales du stockage objet : endpoint, SSL, timeout et liste des buckets.

```bash
dotnet run --project src/Krosoft.ObjectStorage.CLI -- info --profile ./files/local.json
```

**Options**

| Option      | Raccourci | Requis | Description                           |
| ----------- | --------- | ------ | ------------------------------------- |
| `--profile` | `-p`      | oui    | Chemin vers le fichier de profil JSON |

**Exemple de sortie**

```
Endpoint         : http://localhost:9000
SSL              : False
Timeout          : 10s
Buckets          : 3

#    Bucket                                   Création
────────────────────────────────────────────────────────────────────────────────
1    mon-bucket                               2026-01-15 09:00:00
2    archives                                 2026-03-22 14:30:00
3    backups                                  2026-06-01 08:45:00
```

---

### `list`

Liste les fichiers d'un bucket, avec filtrage optionnel par préfixe. Supporte la pagination automatique (>1000 objets).

```bash
dotnet run --project src/Krosoft.ObjectStorage.CLI -- list --profile ./files/local.json --path /archivage
```

**Options**

| Option      | Raccourci | Requis | Description                               |
| ----------- | --------- | ------ | ----------------------------------------- |
| `--profile` | `-p`      | oui    | Chemin vers le fichier de profil JSON     |
| `--path`    | `-d`      | oui    | `bucket` ou `bucket/préfixe/` à parcourir |

**Exemple de sortie**

```
Bucket  : mon-bucket
Préfixe : dossier/
Fichiers: 3

#     Nom                                                          Taille  Modifié
────────────────────────────────────────────────────────────────────────────────────────────────────────
1     rapport-2026.pdf                                            1.2 MB  2026-07-10 14:22:00
2     data.json                                                  45.3 KB  2026-07-11 09:15:00
3     archive.zip                                               512.0 MB  2026-07-12 18:00:00
```

---

### `download`

Télécharge un fichier depuis le stockage objet vers le système de fichiers local. Affiche la progression en temps réel.

```bash
# Téléchargement dans le répertoire courant
dotnet run --project src/Krosoft.ObjectStorage.CLI -- download --profile ./files/local.json --path mon-bucket/dossier/fichier.csv

# Téléchargement dans un dossier (le nom du fichier distant est conservé)
dotnet run --project src/Krosoft.ObjectStorage.CLI -- download --profile ./files/local.json --path archivage/deus/cdar/01a0735c-36a4-751c-ba2f-221d8a45fa11_Rejetee.xml --output ./dumps/

# Téléchargement avancé
dotnet run --project src/Krosoft.ObjectStorage.CLI -- download --profile ./files/local.json --path archivage/peppol/019ef51a-ce8d-73d7-a7a9-86e0d82d060f --output ./dumps/019ef51a-ce8d-73d7-a7a9-86e0d82d060f --decode-base64 --format-xml
```

**Options**

| Option            | Raccourci | Requis | Description                                                                    |
| ----------------- | --------- | ------ | ------------------------------------------------------------------------------ |
| `--profile`       | `-p`      | oui    | Chemin vers le fichier de profil JSON                                          |
| `--path`          | `-d`      | oui    | Chemin du fichier à télécharger au format `bucket/clé`                         |
| `--output`        | `-o`      | non    | Destination locale : chemin de fichier, ou dossier s'il finit par `/` (défaut : répertoire courant). Les dossiers manquants sont créés automatiquement. |
| `--decode-base64` | `-b`      | non    | Décode le contenu depuis Base64 après téléchargement                           |
| `--format-xml`    | `-x`      | non    | Formate le contenu en XML indenté après décodage (nécessite `--decode-base64`) |

**Comportement de `--decode-base64`**

- Le fichier téléchargé est lu, décodé depuis Base64 et réécrit à la destination.
- Si le fichier source a l'extension `.b64`, elle est automatiquement retirée du fichier de sortie (ex: `data.json.b64` → `data.json`).
- En cas de contenu Base64 invalide, une erreur est retournée.

**Comportement de `--format-xml`**

- S'applique après le décodage Base64 (`--decode-base64` requis).
- Parse le contenu décodé comme XML et le réécrit indenté (2 espaces, UTF-8 sans BOM).
- En cas de XML invalide, un avertissement est affiché mais le fichier décodé est conservé.

**Exemple de sortie**

```
  Progression : 100%  (1.2 MB / 1.2 MB)

  Fichier téléchargé en 342 ms → ./dumps/rapport.pdf
```

Avec `--decode-base64` :

```
  Progression : 100%  (45.3 KB / 45.3 KB)

  Fichier téléchargé en 87 ms → ./dumps/data.json.b64
  Décodage Base64 en cours...
  Décodé (33.1 KB) → ./dumps/data.json
```

Avec `--decode-base64 --format-xml` :

```
  Progression : 100%  (18.2 KB / 18.2 KB)

  Fichier téléchargé en 54 ms → ./dumps/invoice.xml.b64
  Décodage Base64 en cours...
  Décodé (13.4 KB) → ./dumps/invoice.xml
  Formatage XML en cours...
  XML formaté (15.1 KB) → ./dumps/invoice.xml
```

---

### `upload`

Envoie un fichier local vers le stockage objet. Opération miroir de `download` : à partir d'un fichier corrigé localement (par ex. téléchargé et décodé/formaté au préalable), on peut le re-normaliser en XML inline puis le ré-encoder en Base64 avant l'envoi. Les transformations sont effectuées **en mémoire** — le fichier local n'est pas modifié.

```bash
# Envoi simple
dotnet run --project src/Krosoft.ObjectStorage.CLI -- upload --profile ./files/local.json --path mon-bucket/dossier/fichier.csv --input ./corrections/fichier.csv

# Renvoi d'un XML corrigé : normalisation inline + ré-encodage Base64 vers la même clé
dotnet run --project src/Krosoft.ObjectStorage.CLI -- upload --profile ./files/prod.json --path archivage/deus/cdar/01a0735c-36a4-751c-ba2f-221d8a45fa11_Rejetee.xml --input ./dumps/01a0735c-36a4-751c-ba2f-221d8a45fa11_Rejetee.xml --format-xml --encode-base64
```

**Options**

| Option            | Raccourci | Requis | Description                                                              |
| ----------------- | --------- | ------ | ------------------------------------------------------------------------ |
| `--profile`       | `-p`      | oui    | Chemin vers le fichier de profil JSON                                    |
| `--path`          | `-d`      | oui    | Destination distante au format `bucket/clé`                              |
| `--input`         | `-i`      | oui    | Chemin du fichier local à envoyer                                        |
| `--format-xml`    | `-x`      | non    | Normalise le contenu XML sur une seule ligne (inline) avant l'envoi      |
| `--encode-base64` | `-b`      | non    | Encode le contenu en Base64 avant l'envoi (appliqué après `--format-xml`) |

**Ordre des transformations**

1. Lecture du fichier `--input`.
2. `--format-xml` : le XML est rechargé et réécrit sur une seule ligne (sans indentation ni espaces de mise en forme), UTF-8 sans BOM. En cas de XML invalide, une erreur est retournée et rien n'est envoyé.
3. `--encode-base64` : le contenu (éventuellement déjà normalisé) est encodé en Base64.
4. Envoi vers `bucket/clé`. Le `Content-Type` est `text/plain` si Base64, sinon `application/xml` pour un XML, sinon `application/octet-stream`.

> `--format-xml` et `--encode-base64` sont indépendants et cumulables ; combinés, ils reproduisent le format attendu par le stockage (XML compact encodé en Base64).

**Exemple de sortie**

```
Source : ./dumps/01a0735c-36a4-751c-ba2f-221d8a45fa11_Rejetee.xml

  Normalisation XML (inline) en cours...
  XML normalisé (13.9 KB)
  Encodage Base64 en cours...
  Encodé (18.5 KB)

  Fichier envoyé en 128 ms → archivage/deus/cdar/01a0735c-36a4-751c-ba2f-221d8a45fa11_Rejetee.xml (18.5 KB)
```
