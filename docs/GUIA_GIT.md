# Guía rápida de Git para el equipo

Las reglas completas están en [REGLAS.md](REGLAS.md). Esto es el paso a paso.

## Una sola vez

```bash
git clone https://github.com/BabyPop666/Kepler7.git
cd Kepler7
dotnet build Kepler7.sln
dotnet test
```

## Identidad en la PC compartida

Trabajamos todos desde la misma PC, así que la identidad se pone **en cada commit**, no en la configuración (si no, queda pegada la del último que la usó):

```bash
git -c user.name="Tu Nombre" -c user.email="el-mail-de-tu-cuenta-de-github" commit -m "feat(...): ..."
```

El mail tiene que ser el de **tu** cuenta de GitHub; si no, tus commits no aparecen a tu nombre en el repo. Antes de pushear, revisá con `git log -3 --format="%an %ae %s"` que el autor sea el correcto.

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

## Varios commits por parte

Un commit por paso que compila, no uno gigante al final. Por ejemplo, para un patrón:

1. `feat(visitor): agrega items del inventario`
2. `feat(visitor): agrega CalculadorPrecio y ContadorArmas`
3. `test(visitor): cubre los tres visitantes`
4. `feat(game): agrega pantalla del Almacen`
5. `docs(uml): diagrama de Visitor`

Antes del PR, lo nuevo tiene que funcionar dentro del juego (ver [REGLAS.md](REGLAS.md), "Cada parte tiene que funcionar").

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

- Cada uno commitea **su** trabajo con **su** identidad. Si dos trabajaron juntos, el mensaje lleva al final `Co-authored-by: Nombre <email>`.
- Nunca `git push --force` sobre `main`.
- Si `main` avanzó mientras trabajabas: `git switch main && git pull && git switch tu-rama && git merge main`, resolvés conflictos y seguís.
- PRs chicos (un patrón, una pantalla) se revisan rápido; PRs gigantes se traban.
