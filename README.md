# Pulga - Saltitando entre cartões

O objetivo da ferramenta é facilitar o consumo e gestão de extratos e cartões, baseado na análise de tráfego da aplicação Bradesco Cartões disponível para Android (`br.com.bradesco.cartoes`), tentando sempre ficar a par da última *release* oficial, que traga alterações no *backend* da aplicação.



### *Debugging* da aplicação produtiva

Para não violar qualquer tipo de licença de uso da aplicação, ou da própria marca, todo o processo de análise de trafego, é baseado somente no monitoramento da comunicação HTTPS com o(s) servidor(es) de *backend* da aplicação. E justamente por esse motivo, torna-se complexo a implementação de todas as *features* existentes no aplicativo, por tanto, essa etapa, também será indexada no repositório, para facilitar colaboradores e mantedores do mesmo, no diretório `helpers`, é fornecido uma série de *scripts* para automatizar a análise dos fluxos, e consequentemente, seu tráfego para abstração na ferramenta.



### Licença

O projeto é licenciado sob *Do What The Fuck You Want To Public License* (WTFPL), e não possui nenhum vinculo com o Bradesco ou qualquer uma de suas marcas.