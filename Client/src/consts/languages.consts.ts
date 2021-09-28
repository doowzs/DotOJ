export interface LanguageInfo {
  code: number;
  factor: number;
  name: string;
  mode: string; // For Cloud Ace9 Editor
  option: string;
}

export const Languages: LanguageInfo[] = [
  {
    code: 1,
    factor: 1.0,
    name: 'C 11',
    mode: 'c_cpp',
    option: '-std=c11 -static -march=native -O2 -fno-strict-aliasing -DONLINE_JUDGE'
  },
  { code: 9, factor: 0.0, name: 'Archive', mode: null, option: '' }
];
