pipeline {
    agent { label 'agent_server' }

    environment {
        DOCKERHUB_CREDENTIALS = credentials('dockerhub-credentials') 
        ZAP_SERVER = credentials('zap-server-url')
        SONAR_SERVER = credentials('sonarqube-server-url')
        STAGING_SERVER = credentials('staging-server-url')
        SNYK_TOKEN = credentials('snyk-api-token')
        GITLAB_TOKEN = credentials('g4_se1818net_token')
    }

    stages {
        stage('Info') {
            steps {
                sh(script: """ whoami;pwd;ls """, label: "first stage")
            }
        }

        
        stage('SonarQube Scan') {
            steps {
                script {
                    dir('sep490-g50-frontend-project') {
                        withSonarQubeEnv('Sonarqube server connection') {
                            sh 'npm install'
                            sh 'npx @sonar/scan -Dsonar.projectKey=sep490-g50-frontend -Dsonar.host.url=$SONAR_SERVER -Dsonar.token=$GITLAB_TOKEN'
                        }
                        sleep 20 
                        def timestamp = new Date().format("yyyyMMdd_HHmmss")
                        env.TIMESTAMP = timestamp  

                        sh "curl -u $GITLAB_TOKEN: \"$SONAR_SERVER/api/issues/search?componentKeys=sep490-g50-frontend&impactSeverities=BLOCKER,HIGH,MEDIUM&statuses=OPEN,CONFIRMED\" -o issues_${timestamp}.json"
                        sh "python3 convert_issue_json.py issues_${timestamp}.json sonarqube-report-${timestamp}.html"
                        archiveArtifacts artifacts: "sonarqube-report-${timestamp}.html", fingerprint: true
                    }
                }
            }
        }

        stage('Snyk Scan') {
            steps {
                dir('sep490-g50-frontend-project') {
                    script {
                        sh 'snyk config set api=${SNYK_TOKEN}'
                        def timestamp = new Date().format("yyyyMMdd_HHmmss")
                        sh """
                            snyk test --json-file-output=snyk.json || true
                            [ -f snyk.json ] && snyk-to-html -i snyk.json -o snyk-report-${timestamp}.html || true
                        """
                        archiveArtifacts artifacts: "snyk-report-${timestamp}.html", fingerprint: true
                    }
                }
            }
        }

        stage('Build Image') {
            steps {
                dir('sep490-g50-frontend-project') {
                    script {
                        sh '''
                            docker build -t sep490-g50-frontend-project .
                            docker tag plsh-fe-librarian co0bridae/sep490-g50-frontend-project:latest
                        '''
                    }
                }
            }
        }

        stage('Trivy Scan') {
            steps {
                script {
                    def timestamp = new Date().format("yyyyMMdd_HHmmss")
                    env.TIMESTAMP = timestamp

                    sh """
                        trivy image --timeout 10m --format json --output sep490-g50-frontend-project-${timestamp}.json sep490-g50-frontend-project:latest
                        python3 convert_json.py sep490-g50-frontend-project-${timestamp}.json sep490-g50-frontend-project-${timestamp}.html
                    """
                    archiveArtifacts artifacts: "sep490-g50-frontend-project-${timestamp}.html", fingerprint: true
                }
            }
        }






        

    }

}

