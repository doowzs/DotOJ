import {
  LanguageInfo,
  VerdictInfo
} from './app.interfaces';


export const Languages: LanguageInfo[] = [
  {
    code: 50, name: 'C', mode: 'c_cpp',
    env: 'GCC 9.2', option: '-DONLINE_JUDGE --static -O2 --std=c11'
  },
  {
    code: 51, name: 'C#', mode: 'csharp',
    env: 'Mono 6.6', option: ''
  },
  {
    code: 54, name: 'C++', mode: 'c_cpp',
    env: 'GCC 9.2', option: '-DONLINE_JUDGE --static -O2 --std=c++17'
  },
  {
    code: 60, name: 'Golang', mode: 'golang',
    env: 'Go 1.13', option: ''
  },
  {
    code: 61, name: 'Haskell', mode: 'haskell',
    env: 'GHC 8.8', option: ''
  },
  {
    code: 62, name: 'Java 11', mode: 'java',
    env: 'OpenJDK 13.0', option: '-J-Xms32m -J-Xmx256m'
  },
  {
    code: 63, name: 'JavaScript', mode: 'javascript',
    env: 'Node.js 12.14', option: ''
  },
  {
    code: 64, name: 'Lua', mode: 'lua',
    env: 'Lua 5.3', option: ''
  },
  {
    code: 68, name: 'PHP', mode: 'php',
    env: 'PHP 7.4', option: ''
  },
  {
    code: 71, name: 'Python 3', mode: 'python',
    env: 'Python 3.8', option: ''
  },
  {
    code: 72, name: 'Ruby', mode: 'ruby',
    env: 'Ruby 2.7', option: ''
  },
  {
    code: 73, name: 'Rust', mode: 'rust',
    env: 'Rust 1.40', option: ''
  },
  {
    code: 74, name: 'TypeScript', mode: 'typescript',
    env: 'TypeScript 3.7', option: ''
  }
];

export const Verdicts: VerdictInfo[] = [
  {
    code: -1, name: 'Service Failed', showCase: false, stage: -1,
    explain: 'An error occurred in the frontend judging service.'
  },
  {
    code: 0, name: 'Pending', showCase: false, stage: 0,
    explain: 'Your code is submitted and is waiting to be processed.'
  },
  {
    code: 1, name: 'In Queue', showCase: false, stage: 0,
    explain: 'Your code is waiting in queue to be processed by the judging service.'
  },
  {
    code: 2, name: 'Running', showCase: true, stage: 0,
    explain: 'Your program is running on a test case by the judging service.'
  },
  {
    code: 3, name: 'Accepted', showCase: false, stage: 1,
    explain: 'Your program passed all test cases.'
  },
  {
    code: 4, name: 'Wrong Answer', showCase: true, stage: 2,
    explain: 'The output of your program does not match expected output.'
  },
  {
    code: 5, name: 'Time Limit Exceeded', showCase: true, stage: 2,
    explain: 'Your program did not terminate before hitting the time limit.'
  },
  {
    code: 6, name: 'Compilation Error', showCase: false, stage: 2,
    explain: 'Your code cannot compile.'
  },
  {
    code: 7, name: 'Runtime Error', showCase: true, stage: 2,
    explain: 'Your program did not exit normally or hit the memory limit.'
  },
  {
    code: 8, name: 'Runtime Error', showCase: true, stage: 2,
    explain: 'Your program did not exit normally or hit the memory limit.'
  },
  {
    code: 9, name: 'Runtime Error', showCase: true, stage: 2,
    explain: 'Your program did not exit normally or hit the memory limit.'
  },
  {
    code: 10, name: 'Runtime Error', showCase: true, stage: 2,
    explain: 'Your program did not exit normally or hit the memory limit.'
  },
  {
    code: 11, name: 'Runtime Error', showCase: true, stage: 2,
    explain: 'Your program did not exit normally or hit the memory limit.'
  },
  {
    code: 12, name: 'Runtime Error', showCase: true, stage: 2,
    explain: 'Your program did not exit normally or hit the memory limit.'
  },
  {
    code: 13, name: 'Internal Error', showCase: false, stage: -1,
    explain: 'An error occurred in the backend judging service.'
  },
  {
    code: 14, name: 'Exec Format Error', showCase: false, stage: 2,
    explain: 'Your program has invalid executable format.'
  }
];
