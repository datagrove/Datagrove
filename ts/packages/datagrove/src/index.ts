import yargs from 'yargs'
import dotenv from 'dotenv'
import chalk from 'chalk'
import inquirer from 'inquirer'

export function main() {
  dotenv.config()

  yargs
    .scriptName('dg')
    .usage("$0 command")
    .version('0.1')
    .command({
      command: 'hello',
      describe: 'nothing',
      handler: async parsed => {
        var s = await inquirer.prompt({
          name: "yoname",
          message: "wassup?",
          default: "not much"
        })
        console.log(chalk.bgGreen(`hello ${s.yoname}`))
      },
    })
    .help()
    .demandCommand()
    .argv
}
main()