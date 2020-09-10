export interface LanguageInfo {
  code: number;
  factor: number;
  name: string;
  mode: string; // For Cloud Ace9 Editor
  option: string;
}

export const Languages: LanguageInfo[] = [
  { code: 1, factor: 1.0, name: 'C 11', mode: 'c_cpp', option: '-DONLINE_JUDGE --static -O2 --std=c11' },
  { code: 2, factor: 1.0, name: 'C++ 17', mode: 'c_cpp', option: '-DONLINE_JUDGE --static -O2 --std=c++17' },
  { code: 3, factor: 2.0, name: 'Java 11', mode: 'java', option: '-J-Xms32m -J-Xmx256m' },
  { code: 4, factor: 5.0, name: 'Python 3', mode: 'python', option: '' },
  { code: 5, factor: 2.0, name: 'Golang', mode: 'golang', option: '' },
  /*
  {
    code: 51, factor: 1.5, name: 'C#', mode: 'csharp',
    env: 'Mono 6.6', option: ''
  },
  {
    code: 61, factor: 2.5, name: 'Haskell', mode: 'haskell',
    env: 'GHC 8.8', option: ''
  },
  {
    code: 63, factor: 5.0, name: 'JavaScript', mode: 'javascript',
    env: 'Node.js 12.14', option: ''
  },
  {
    code: 64, factor: 6.0, name: 'Lua', mode: 'lua',
    env: 'Lua 5.3', option: ''
  },
  {
    code: 68, factor: 4.5, name: 'PHP', mode: 'php',
    env: 'PHP 7.4', option: ''
  },
  {
    code: 72, factor: 5.0, name: 'Ruby', mode: 'ruby',
    env: 'Ruby 2.7', option: ''
  },
  {
    code: 73, factor: 2.5, name: 'Rust', mode: 'rust',
    env: 'Rust 1.40', option: ''
  },
  {
    code: 74, factor: 5.0, name: 'TypeScript', mode: 'typescript',
    env: 'TypeScript 3.7', option: ''
  }
  */
];
