# KAY ONE — Financial & Business Operating System

Application Windows de gestion financière intégrée pour KAY Groupe. L’interface reprend le dossier Stitch fourni avec une palette sobre bleu/gris et réserve le vert, l’orange et le rouge aux états métier.

## Démarrage rapide

### Version livrée

1. Ouvrez le dossier `KAY-ONE-Client-MVP-1.0.0-win-x64`.
2. Double-cliquez sur `KayOne.exe`.
3. Au premier démarrage, créez l’administrateur avec un mot de passe d’au moins 12 caractères.
4. Ajoutez la clé affichée dans Microsoft Authenticator, Google Authenticator ou 1Password, puis saisissez le code à six chiffres.

Microsoft Edge WebView2 Runtime est requis. Il est inclus par défaut dans Windows 11 et les versions récentes de Windows 10.

### Depuis les sources

```powershell
dotnet restore freelanceProject1.csproj
dotnet run --project freelanceProject1.csproj
```

Le projet cible `.NET 10` sous Windows.

## Données et sauvegarde

Le magasin local se trouve dans :

```text
%LocalAppData%\KayOne\enterprise-data.json
```

Chaque écriture crée une sauvegarde atomique `enterprise-data.json.bak`. Sauvegardez ensemble les deux fichiers avant une mise à jour. Pour repartir avec une installation vierge, fermez KAY ONE et déplacez ces fichiers dans un dossier d’archive.

## Fonctionnalités intégrées

- Transaction Engine : une saisie reliée aux documents, factures, échéances, fiscalité, comptabilité, trésorerie, banque, reporting et audit.
- Workflow brouillon → soumission → validation → paiement/rapprochement → comptabilisation, avec annulation tracée.
- Nouvelle opération adaptative : 14 types, dimensions analytiques, devises, change, import, immobilisation, fiscalité et justificatifs.
- Ventes, achats, Clients 360°, Fournisseurs 360°, contrats, dossiers import et certificats d’exonération.
- Paiements avec séparation préparateur/validateur, banques, rapprochement, caisses et balances âgées clients/fournisseurs.
- Règles TVA/RAS versionnées, écritures équilibrées, immobilisations/amortissements, commissions et notes de frais.
- Recherche globale par référence, tiers, montant, IBAN et objets liés, avec vue de traçabilité.
- Authentification locale PBKDF2, MFA TOTP, rôles, périmètres société, expiration de session et audit chaîné par empreinte.
- Tableaux de bord et assistant déterministe alimentés uniquement par le snapshot autorisé.

Les justificatifs et imports sans contenu binaire sont enregistrés comme « en attente du fichier » : l’interface ne prétend pas qu’un fichier a été stocké ou traité par OCR lorsqu’il ne l’a pas été.

## Vérification automatique

Test complet moteur + interface :

```powershell
$env:KAYONE_UI_SMOKE_TEST = '1'
$env:KAYONE_TEST_REPORT = "$PWD\artifacts\ui-smoke-manual.json"
$env:KAYONE_DATA_FILE = "$PWD\artifacts\ui-data-manual.json"
dotnet run -c Release --project freelanceProject1.csproj
```

Test moteur seul :

```powershell
$env:KAYONE_ENGINE_TEST = '1'
$env:KAYONE_TEST_REPORT = "$PWD\artifacts\engine-manual.json"
$env:KAYONE_DATA_FILE = "$PWD\artifacts\engine-data-manual.json"
dotnet run -c Release --project freelanceProject1.csproj
```

## Positionnement de cette livraison

Cette version est un MVP client autonome et testable sur un poste Windows. Avant un déploiement multi-utilisateur réel, il reste nécessaire de remplacer le stockage JSON local par l’architecture serveur prévue au cahier des charges : API, PostgreSQL, stockage objet S3, service OCR, chiffrement centralisé, sauvegarde/PRA, supervision et tests de charge/sécurité sur l’infrastructure cible.

La couverture détaillée et les réserves se trouvent dans `REQUIREMENTS-MATRIX.md`.
