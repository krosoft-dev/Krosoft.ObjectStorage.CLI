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

| Champ | Type | Requis | Description |
|-------|------|--------|-------------|
| `endpoint` | `string` | oui | URL de l'instance S3/MinIO |
| `accessKey` | `string` | oui | Clé d'accès |
| `secretKey` | `string` | oui | Clé secrète |
| `useSSL` | `bool` | non | Active HTTPS (défaut : `false`) |
| `timeout` | `int` | non | Timeout en secondes (défaut : `10`) |

---

## Commandes

### `info`

Affiche les informations générales du stockage objet : endpoint, SSL, timeout et liste des buckets.

```bash
.\src\Krosoft.ObjectStorage.CLI\bin\Debug\net10.0\Krosoft.ObjectStorage.CLI.exe info --profile ./files/local.json
```

**Options**

| Option | Raccourci | Requis | Description |
|--------|-----------|--------|-------------|
| `--profile` | `-p` | oui | Chemin vers le fichier de profil JSON |

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

.\src\Krosoft.ObjectStorage.CLI\bin\Debug\net10.0\Krosoft.ObjectStorage.CLI.exe info --profile ./files/local.json
.\src\Krosoft.ObjectStorage.CLI\bin\Debug\net10.0\Krosoft.ObjectStorage.CLI.exe list --profile ./files/local.json  --path /archivage
```

**Options**

| Option | Raccourci | Requis | Description |
|--------|-----------|--------|-------------|
| `--profile` | `-p` | oui | Chemin vers le fichier de profil JSON |
| `--path` | `-d` | oui | `bucket` ou `bucket/préfixe/` à parcourir |

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
.\src\Krosoft.ObjectStorage.CLI\bin\Debug\net10.0\Krosoft.ObjectStorage.CLI.exe download --profile ./files/local.json --path mon-bucket/dossier/fichier.csv



 
# Destination explicite
.\src\Krosoft.ObjectStorage.CLI\bin\Debug\net10.0\Krosoft.ObjectStorage.CLI.exe download --profile ./files/local.json  --path archivage/peppol/019ef51a-ce8d-73d7-a7a9-86e0d82d060f --output ./dumps/019ef51a-ce8d-73d7-a7a9-86e0d82d060f --decode-base64
 

# Téléchargement + décodage Base64 vers un chemin précis
.\src\Krosoft.ObjectStorage.CLI\bin\Debug\net10.0\Krosoft.ObjectStorage.CLI.exe download --profile ./files/local.json  --path archivage/peppol/019ef51a-ce8d-73d7-a7a9-86e0d82d060f --output ./dumps/019ef51a-ce8d-73d7-a7a9-86e0d82d060f --decode-base64
```

**Options**

| Option | Raccourci | Requis | Description |
|--------|-----------|--------|-------------|
| `--profile` | `-p` | oui | Chemin vers le fichier de profil JSON |
| `--path` | `-d` | oui | Chemin du fichier à télécharger au format `bucket/clé` |
| `--output` | `-o` | non | Chemin local de destination (défaut : répertoire courant) |
| `--decode-base64` | `-b` | non | Décode le contenu depuis Base64 après téléchargement |

**Comportement de `--decode-base64`**

- Le fichier téléchargé est lu, décodé depuis Base64 et réécrit à la destination.
- Si le fichier source a l'extension `.b64`, elle est automatiquement retirée du fichier de sortie (ex: `data.json.b64` → `data.json`).
- En cas de contenu Base64 invalide, une erreur est retournée.

**Exemple de sortie**

```
  Progression : 100%  (1.2 MB / 1.2 MB)

  Fichier téléchargé en 342 ms → C:\Téléchargements\rapport.pdf
```

Avec `--decode-base64` :

```
  Progression : 100%  (45.3 KB / 45.3 KB)

  Fichier téléchargé en 87 ms → C:\out\data.json.b64
  Décodage Base64 en cours...
  Décodé (33.1 KB) → C:\out\data.json
```
