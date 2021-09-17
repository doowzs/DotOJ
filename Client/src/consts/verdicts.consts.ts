export enum VerdictStage {
  ERROR, RUNNING, ACCEPTED, REJECTED
}

export interface VerdictInfo {
  code: number;
  name: string;
  showCase: boolean;
  stage: VerdictStage;
  color: string;
  explain: string;
}

export const Verdicts: VerdictInfo[] = [
  {
    code: -3, name: '拒绝评测', showCase: false, stage: VerdictStage.REJECTED,
    color: 'secondary', explain: '这份提交的代码无法被评测机评测，因此被拒绝。'
  },
  {
    code: -2, name: '提交作废', showCase: false, stage: VerdictStage.ERROR,
    color: 'secondary', explain: '这份提交被一个新的提交覆盖或被管理员手动作废。'
  },
  {
    code: -1, name: '评测失败', showCase: false, stage: VerdictStage.ERROR,
    color: 'secondary', explain: '评测服务遇到内部错误，无法评测代码。'
  },
  {
    code: 0, name: '等待评测', showCase: false, stage: VerdictStage.RUNNING,
    color: 'primary', explain: '你的代码已提交，正在等待评测。'
  },
  /* code=1 is now deprecated
  {
    code: 1, name: '等待队列', showCase: false, stage: VerdictStage.RUNNING,
    color: 'primary', explain: '你的代码已提交且已经进入评测队列，正在等待评测。'
  },
  */
  {
    code: 2, name: '正在运行', showCase: true, stage: VerdictStage.RUNNING,
    color: 'primary', explain: '你的程序正在测试数据上运行。'
  },
  {
    code: 3, name: '答案正确', showCase: false, stage: VerdictStage.ACCEPTED,
    color: 'success', explain: '你的程序通过了所有的测试用例。'
  },
  {
    code: 4, name: '答案错误', showCase: true, stage: VerdictStage.REJECTED,
    color: 'danger', explain: '你的程序在某些测试用例上输出的答案不正确。'
  },
  {
    code: 5, name: '时间超限', showCase: true, stage: VerdictStage.REJECTED,
    color: 'danger', explain: '你的程序在某些测试用例上没有及时结束运行。'
  },
  {
    code: 6, name: '内存超限', showCase: true, stage: VerdictStage.REJECTED,
    color: 'danger', explain: '你的程序在某些测试用例上使用了超出限制的内存。'
  },
  {
    code: 7, name: '编译错误', showCase: false, stage: VerdictStage.REJECTED,
    color: 'danger', explain: '你提交的代码无法编译。'
  },
  {
    code: 8, name: '运行错误', showCase: true, stage: VerdictStage.REJECTED,
    color: 'danger', explain: '你的程序没有以0为返回值退出。'
  },
  {
    code: 9, name: '测试完成', showCase: false, stage: VerdictStage.ACCEPTED,
    color: 'secondary', explain: '你提交的自定义测试成功完成。'
  }
];
