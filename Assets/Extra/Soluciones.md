# Soluciones de Puzzles - Game Aletheia Cross

Guía completa con las soluciones a todos los puzzles de programación del juego.

---

## **NIVEL 1: El Despertar Digital**

### Puzzle: "Bienvenida Digital"
**Tipo**: Tutorial  
**Puntos**: 50  
**Objetivo**: Imprimir "Bienvenido a Aletheia"
```java
public class Main {
    public static void main(String[] args) {
        System.out.println("Bienvenido a Aletheia");
    }
}
```

**Salida esperada**: `Bienvenido a Aletheia`

---

## **NIVEL 2: Ruinas del Firewall Antiguo**

### Puzzle: "Suma de Dos Números"
**Tipo**: Aritmética  
**Puntos**: 75  
**Objetivo**: Sumar 15 y 27, imprimir el resultado
```java
public class Main {
    public static void main(String[] args) {
        int a = 15;
        int b = 27;
        int suma = a + b;
        System.out.println(suma);
    }
}
```

**Salida esperada**: `42`

---

## **NIVEL 3: Ciudad de las Contraseñas Perdidas**

### Puzzle 1: "Detector de Pares"
**Tipo**: Condicionales  
**Puntos**: 100  
**Objetivo**: Verificar si 8 es par o impar
```java
public class Main {
    public static void main(String[] args) {
        int numero = 8;
        if (numero % 2 == 0) {
            System.out.println("PAR");
        } else {
            System.out.println("IMPAR");
        }
    }
}
```

**Salida esperada**: `PAR`

---

### Puzzle 2: "Clasificador de Edad"
**Tipo**: Condicionales Múltiples  
**Puntos**: 120  
**Objetivo**: Clasificar edad 25 como MENOR/ADULTO/SENIOR
```java
public class Main {
    public static void main(String[] args) {
        int edad = 25;
        if (edad < 18) {
            System.out.println("MENOR");
        } else if (edad >= 18 && edad <= 65) {
            System.out.println("ADULTO");
        } else {
            System.out.println("SENIOR");
        }
    }
}
```

**Salida esperada**: `ADULTO`

---

## **NIVEL 4: Laberinto de Algoritmos**

### Puzzle 1: "Conexión de Nodos Digitales"
**Tipo**: Bucles  
**Puntos**: 125  
**Objetivo**: Imprimir números del 1 al 5, cada uno en una línea
```java
public class Main {
    public static void main(String[] args) {
        for (int i = 1; i <= 5; i++) {
            System.out.println(i);
        }
    }
}
```

**Salida esperada**:
```
1
2
3
4
5
```

---

### Puzzle 2: "Suma de Energía del Laberinto"
**Tipo**: Bucles con Acumulador  
**Puntos**: 150  
**Objetivo**: Sumar números del 1 al 10
```java
public class Main {
    public static void main(String[] args) {
        int suma = 0;
        for (int i = 1; i <= 10; i++) {
            suma += i;
        }
        System.out.println(suma);
    }
}
```

**Salida esperada**: `55`

---

## **NIVEL 5: Santuario de los Datos Sagrados**

### Puzzle 1: "Factorial Recursivo"
**Tipo**: Recursión  
**Puntos**: 200  
**Objetivo**: Calcular el factorial de 5
```java
public class Main {
    public static int factorial(int n) {
        if (n == 0 || n == 1) {
            return 1;
        }
        return n * factorial(n - 1);
    }
    
    public static void main(String[] args) {
        System.out.println(factorial(5));
    }
}
```

**Salida esperada**: `120`

---

### Puzzle 2: "Potencia Recursiva"
**Tipo**: Recursión Avanzada  
**Puntos**: 225  
**Objetivo**: Calcular 2 elevado a la 8
```java
public class Main {
    public static int potencia(int base, int exponente) {
        if (exponente == 0) {
            return 1;
        }
        return base * potencia(base, exponente - 1);
    }
    
    public static void main(String[] args) {
        System.out.println(potencia(2, 8));
    }
}
```

**Salida esperada**: `256`

---

## **NIVEL 6: Torre Corporativa Redline**

### Puzzle 1: "Array de Datos Corporativos"
**Tipo**: Arrays  
**Puntos**: 175  
**Objetivo**: Encontrar el máximo en el array {3, 7, 1, 9, 2}
```java
public class Main {
    public static void main(String[] args) {
        int[] datos = {3, 7, 1, 9, 2};
        int max = datos[0];
        for (int i = 1; i < datos.length; i++) {
            if (datos[i] > max) {
                max = datos[i];
            }
        }
        System.out.println(max);
    }
}
```

**Salida esperada**: `9`

---

### Puzzle 2: "Inversor de Array"
**Tipo**: Manipulación de Arrays  
**Puntos**: 200  
**Objetivo**: Invertir el array {1, 2, 3, 4, 5}
```java
public class Main {
    public static void main(String[] args) {
        int[] arr = {1, 2, 3, 4, 5};
        for (int i = arr.length - 1; i >= 0; i--) {
            System.out.print(arr[i]);
            if (i > 0) {
                System.out.print(" ");
            }
        }
    }
}
```

**Salida esperada**: `5 4 3 2 1`

---

### Puzzle 3: "Búsqueda en Array"
**Tipo**: Búsqueda  
**Puntos**: 220  
**Objetivo**: Encontrar el índice del número 30 en {10, 20, 30, 40, 50}
```java
public class Main {
    public static void main(String[] args) {
        int[] arr = {10, 20, 30, 40, 50};
        int buscar = 30;
        int indice = -1;
        for (int i = 0; i < arr.length; i++) {
            if (arr[i] == buscar) {
                indice = i;
                break;
            }
        }
        System.out.println(indice);
    }
}
```

**Salida esperada**: `2`

---

## **NIVEL 7: El Archivo Prohibido**

### Puzzle 1: "Secuencia de Fibonacci"
**Tipo**: Algoritmos  
**Puntos**: 250  
**Objetivo**: Imprimir los primeros 7 números de Fibonacci
```java
public class Main {
    public static void main(String[] args) {
        int a = 0;
        int b = 1;
        System.out.print(a + " " + b);
        for (int i = 2; i < 7; i++) {
            int siguiente = a + b;
            System.out.print(" " + siguiente);
            a = b;
            b = siguiente;
        }
    }
}
```

**Salida esperada**: `0 1 1 2 3 5 8`

---

### Puzzle 2: "Número Primo"
**Tipo**: Algoritmos Avanzados  
**Puntos**: 280  
**Objetivo**: Verificar si 17 es primo
```java
public class Main {
    public static void main(String[] args) {
        int numero = 17;
        boolean esPrimo = true;
        if (numero <= 1) {
            esPrimo = false;
        } else {
            for (int i = 2; i <= numero / 2; i++) {
                if (numero % i == 0) {
                    esPrimo = false;
                    break;
                }
            }
        }
        if (esPrimo) {
            System.out.println("PRIMO");
        } else {
            System.out.println("NO PRIMO");
        }
    }
}
```

**Salida esperada**: `PRIMO`

---

### Puzzle 3: "Palíndromo Numérico"
**Tipo**: Algoritmos de Cadenas  
**Puntos**: 300  
**Objetivo**: Verificar si 12321 es palíndromo
```java
public class Main {
    public static void main(String[] args) {
        int numero = 12321;
        String str = String.valueOf(numero);
        String reverso = new StringBuilder(str).reverse().toString();
        if (str.equals(reverso)) {
            System.out.println("SI");
        } else {
            System.out.println("NO");
        }
    }
}
```

**Salida esperada**: `SI`

---

### Puzzle 4: "Ordenamiento Burbuja"
**Tipo**: Algoritmos de Ordenamiento  
**Puntos**: 350  
**Objetivo**: Ordenar el array {5, 2, 8, 1, 9} usando burbuja
```java
public class Main {
    public static void main(String[] args) {
        int[] arr = {5, 2, 8, 1, 9};
        for (int i = 0; i < arr.length - 1; i++) {
            for (int j = 0; j < arr.length - 1 - i; j++) {
                if (arr[j] > arr[j + 1]) {
                    int temp = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = temp;
                }
            }
        }
        for (int i = 0; i < arr.length; i++) {
            System.out.print(arr[i]);
            if (i < arr.length - 1) {
                System.out.print(" ");
            }
        }
    }
}
```

**Salida esperada**: `1 2 5 8 9`

---

## Resumen de Puntuación

| Nivel | Nombre del Nivel | Puzzles | Puntos Totales |
|-------|------------------|---------|----------------|
| 1 | El Despertar Digital | 1 | 50 |
| 2 | Ruinas del Firewall Antiguo | 1 | 75 |
| 3 | Ciudad de las Contraseñas Perdidas | 2 | 220 |
| 4 | Laberinto de Algoritmos | 2 | 275 |
| 5 | Santuario de los Datos Sagrados | 2 | 425 |
| 6 | Torre Corporativa Redline | 3 | 595 |
| 7 | El Archivo Prohibido | 4 | 1,180 |

**Puntuación máxima total de puzzles**: 2,820 puntos

---

## Notas Importantes

1. **Salida exacta**: El sistema valida que la salida sea exactamente igual a la esperada (incluyendo espacios y saltos de línea).

2. **Compilación**: Todos los códigos deben compilar correctamente con `javac` y ejecutarse con `java`.

3. **Clase Main**: Siempre debe llamarse `Main` con la primera letra mayúscula.

4. **Método main**: Debe tener la firma exacta: `public static void main(String[] args)`

5. **Sistema de pistas**: Cada puzzle tiene 3 pistas disponibles que puedes usar sin penalización.

6. **Progreso**: Debes completar todos los puzzles de un nivel antes de avanzar al siguiente.

---

## Consejos Generales

- Lee cuidadosamente la descripción del puzzle
- Usa las pistas si te atascas
- Prueba tu código mentalmente antes de compilar
- Revisa los ejemplos de salida esperada
- Los espacios y saltos de línea son importantes en la validación