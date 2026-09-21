# Guía rápida de Git para el equipo

Las reglas completas están en [REGLAS.md](REGLAS.md). Esto es el paso a paso.

## Una sola vez

```bash
git clone https://github.com/BabyPop666/Kepler7.git
cd Kepler7
git config user.name  "Tu Nombre"
git config user.email "el-mail-de-tu-cuenta-de-github"
dotnet build Kepler7.sln
dotnet test
```

El mail tiene que ser el de **tu** cuenta de GitHub; si no, tus commits no aparecen a tu nombre en el repo.

## Cada vez que trabajás

```bash
git switch main
git pull
git switch -c feature/nombre-del-patron      # ej.: feature/chain-defensas

# ...programás, y vas commiteando de a pasos chicos...
git add .
git commit -m "feat(chain): agrega eslabones Esquiva y Bloqueo"

git push -u origin feature/nombre-del-patron
```

Después abrís el **Pull Request** en GitHub, le pedís revisión a otro integrante y, cuando lo aprueba y el CI está en verde, se mergea a `main`.

## Mensajes de commit

`tipo(tema): qué hace`, en español y en presente.

| Tipo | Para qué |
|---|---|
| `feat` | funcionalidad nueva |
| `fix` | arreglo de un bug |
| `test` | pruebas |
| `docs` | UML, informe, README |
| `refactor` | reordenar código sin cambiar comportamiento |
| `chore` | configuración, CI, limpieza |

Ejemplos: `feat(proxy): registra intentos denegados`, `test(state): cubre transición a Berserk`, `docs(uml): diagrama de Visitor`.

## Reglas de oro

- Cada uno commitea **su** trabajo desde **su** cuenta. Si dos trabajaron juntos, el mensaje lleva al final `Co-authored-by: Nombre <email>`.
- Nunca `git push --force` sobre `main`.
- Si `main` avanzó mientras trabajabas: `git switch main && git pull && git switch tu-rama && git merge main`, resolvés conflictos y seguís.
- PRs chicos (un patrón, una pantalla) se revisan rápido; PRs gigantes se traban.
