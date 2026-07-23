import js from '@eslint/js';
import globals from 'globals';
import reactHooks from 'eslint-plugin-react-hooks';
import reactRefresh from 'eslint-plugin-react-refresh';
import pluginImport from "eslint-plugin-import";
import { globalIgnores } from "eslint/config";

export default [
  { ignores: ['dist'] },
  {
    files: ['**/*.{js,jsx}'],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
      parserOptions: {
        ecmaVersion: 'latest',
        ecmaFeatures: { jsx: true },
        sourceType: 'module',
      },
    },
    plugins: {
      'react-hooks': reactHooks,
      'react-refresh': reactRefresh,
      pluginImport
    },
    rules: {
      ...js.configs.recommended.rules,
      ...reactHooks.configs.recommended.rules,
      'no-unused-vars': ['error', { varsIgnorePattern: '^[A-Z_]', caughtErrors: "all", caughtErrorsIgnorePattern: "^_" }],
      'react-refresh/only-export-components': [
        'warn',
        { allowConstantExport: true },
      ],

      // geral
      "semi": "error", // obriga a usar ; em todo lugar.
      "eqeqeq": "error", // devemos obrigar sempre a usar === em vez de ==, para evitar algumas situações onde comparações retornam verdadeiro mesmo que não faça sentido logicamente.
      "no-eval": "error", // desabilitado, questão de segurança.
      "no-labels": "error", // uma forma de goto, e não usamos isso!

      // tipagem
      "no-implicit-coercion": "error", // fazer a conversão de tipos do jeito certo.

      // variaveis
			"no-var": "error", // devemos proibir var no nosso codigo.
      "prefer-const": "error", // variaveis que não são re-assignadas devem ser const.
      "no-const-assign": "error", // nao pode re-assignar uma const né.
      "no-use-before-define": "error", // Não pode referenciar variaveis que ainda não foram definidas. basicamente, evitando as armadilhas do "hoisting" do javascript, simplificando a linguagem também.
      "no-undef": "error", // não permite referenciar variaveis (var?) que não existem.
      "one-var": ["error", "never"], // não permite criar multiplas variaveis em apenas uma linha.
      "no-multi-assign": "error", // não permitir assignar valores em multiplas variaveis de uma vez.

      // strings
      "prefer-template": "error", // nao aceitar concatenação de string com +, e sim usar `${}` para formatar uma string.
      "no-useless-concat": "error", // complementa a de cima, não deixar concatenações inuteis acontecer. "hello" + "world" proibido.

      // modules
      "no-duplicate-imports": "error", // desabilita imports duplicados.
      "pluginImport/no-mutable-exports": "error", // nao pode exportar let, apenas const.
      "pluginImport/named": "error", // verifica se o nome bate com o caminho.
      "pluginImport/first": "error" // permite apenas fazer o import no topo do modulo.
    },
    ignores: [
      // for non-global ignores a full glob pattern is required
      '**/android/*',
      '**/ios/*',
    ],
  },
  globalIgnores(
    ["android/", "ios/"]
  ),
];
